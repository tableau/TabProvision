using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Provisioning insturcitons on which users to add and and what to do with missing users
/// </summary>
internal partial class ProvisionUserInstructions
{
    //Sets of users/roles and groups to provision
    public readonly ReadOnlyCollection<ProvisioningUser> UsersToProvision;
    public readonly ReadOnlyCollection<ProvisioningGroup> GroupsToProvision;

    //Instructions on how to handle updates to the Tableau Site
    //Unexpected site users
    public readonly UnexpectedUserAction ActionForUnexpectedSamlUsers;
    public readonly UnexpectedUserAction ActionForUnexpectedDefaultAuthUsers;
    //Site missing users needing provisioning
    public readonly MissingUserAction ActionForMissingSamlUsers;
    public readonly MissingUserAction ActionForMissingDefaultAuthUsers;
    //Site existing users needing modification
    public readonly ExistingUserAction ActionForExistingSamlUsers;
    public readonly ExistingUserAction ActionForExistingDefaultAuthUsers;

    //Group membership
    public readonly UnexpectedGroupMemberAction ActionForUnexpectedGroupMembers;
    public readonly MissingGroupMemberAction ActionForMissingGroupMembers;

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="filePath"></param>
    public ProvisionUserInstructions(string filePath)
    {
        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfigTargetSite = new System.Xml.XmlDocument();
        xmlConfigTargetSite.Load(filePath);

        //-------------------------------------------------------------------------------
        //Get instructions about the intended site membership provisioning
        //-------------------------------------------------------------------------------
        var xnodeSiteMembership = xmlConfigTargetSite.SelectSingleNode("//SiteProvisioning/SiteMembership");
        this.ActionForUnexpectedSamlUsers = ParseUnexpectedUserAction(
                                        xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authSamlUnexpectedUsers].Value);

        this.ActionForUnexpectedDefaultAuthUsers = ParseUnexpectedUserAction(
                                xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authDefaultUnexpectedUsers].Value);

        this.ActionForMissingSamlUsers = ParseMissingUserAction(
                                                xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authSamlMissingUsers].Value);

        this.ActionForMissingDefaultAuthUsers = ParseMissingUserAction(
                                                xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authDefaultMissingUsers].Value);

        this.ActionForExistingSamlUsers = ParseExistingUserAction(
                                        xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authSamlExistingUsers].Value);

        this.ActionForExistingDefaultAuthUsers = ParseExistingUserAction(
                                                xnodeSiteMembership.Attributes[ProvisionUserInstructions.XmlAttribute_authDefaultExistingUsers].Value);

        //-------------------------------------------------------------------------------
        //Get the users
        //-------------------------------------------------------------------------------
        this.UsersToProvision = ParseUsers(xmlConfigTargetSite, "//SiteProvisioning/SiteMembership/User").AsReadOnly();

        //-------------------------------------------------------------------------------
        //Groups' memberships
        //-------------------------------------------------------------------------------
        var xnodeGroups = xmlConfigTargetSite.SelectSingleNode("//SiteProvisioning/GroupsMemberships");
        this.ActionForMissingGroupMembers = ParseMissingGroupMemberAction(
                                                xnodeGroups.Attributes[ProvisionUserInstructions.XmlAttribute_MissingGroupMembers].Value);

        this.ActionForUnexpectedGroupMembers = ParseUnexpectedGroupMemberAction(
                                        xnodeGroups.Attributes[ProvisionUserInstructions.XmlAttribute_UnexpectedGroupMembers].Value);

        //Load each group's membership information
        var provisionGroups = new List<ProvisioningGroup>();
        var xnodesGroups = xmlConfigTargetSite.SelectNodes("//SiteProvisioning/GroupsMemberships/GroupMembership");
        foreach(XmlNode xmlThisGroup in xnodesGroups)
        {
            var thisGroup = new ProvisioningGroup(xmlThisGroup);
            provisionGroups.Add(thisGroup);
        }
        this.GroupsToProvision = provisionGroups.AsReadOnly();

    }

    /// <summary>
    /// Parse all the users from the xml document
    /// </summary>
    /// <param name="xDoc"></param>
    /// <returns></returns>
    public static List<ProvisioningUser> ParseUsers(XmlDocument xDoc, string xPath, string overrideSourceGroup = null)
    {
        var xmlUsersToProvision = xDoc.SelectNodes(xPath);
        var listOut = new List<ProvisioningUser>();
        foreach(XmlNode xNode in xmlUsersToProvision)
        {
            var thisUser = new ProvisioningUser(xNode, overrideSourceGroup);
            listOut.Add(thisUser);
        }

        return listOut;
    }

}