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
    public readonly UnexpectedUserAction ActionForUnexpectedOpenIdUsers;
    //Site missing users needing provisioning
    public readonly MissingUserAction ActionForMissingSamlUsers;
    public readonly MissingUserAction ActionForMissingDefaultAuthUsers;
    public readonly MissingUserAction ActionForMissingOpenIdUsers;
    //Site existing users needing modification
    public readonly ExistingUserAction ActionForExistingSamlUsers;
    public readonly ExistingUserAction ActionForExistingDefaultAuthUsers;
    public readonly ExistingUserAction ActionForExistingOpenIdUsers;

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
        //UNEXPECTED USERS
        //1.
        this.ActionForUnexpectedSamlUsers = ParseUnexpectedUserActionFromAttribute(
                                        xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authSamlUnexpectedUsers);
        //2.
        this.ActionForUnexpectedDefaultAuthUsers = ParseUnexpectedUserActionFromAttribute(
                                xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authDefaultUnexpectedUsers);
        //3.
        this.ActionForUnexpectedOpenIdUsers = ParseUnexpectedUserActionFromAttribute(
                                        xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authOpenIdUnexpectedUsers);



        //MISSING USERS
        //1.
        this.ActionForMissingSamlUsers = ParseMissingUserActionFromAttribute(
                                                xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authSamlMissingUsers);
        //2.
        this.ActionForMissingDefaultAuthUsers = ParseMissingUserActionFromAttribute(
                                                xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authDefaultMissingUsers);
        //3.
        this.ActionForMissingOpenIdUsers = ParseMissingUserActionFromAttribute(
                                                xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authOpenIdMissingUsers);


        //EXISTING USERS NEEDING MODIFICATION
        //1.
        this.ActionForExistingSamlUsers = ParseExistingUserActionFromAttribute(
                                        xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authSamlExistingUsers);
        //2.
        this.ActionForExistingDefaultAuthUsers = ParseExistingUserActionFromAttribute(
                                         xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authDefaultExistingUsers);
        //3.
        this.ActionForExistingOpenIdUsers = ParseExistingUserActionFromAttribute(
                                        xnodeSiteMembership, ProvisionUserInstructions.XmlAttribute_authOpenIdExistingUsers);

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