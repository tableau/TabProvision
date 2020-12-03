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
    public readonly ReadOnlyCollection<ProvisioningChangeContentOwnership> ContentOwnershipToProvision;

    //Instructions on how to handle updates to the Tableau Site
    //Unexpected site users
    public readonly UnexpectedUserAction ActionForUnexpectedSamlUsers = UnexpectedUserAction.Report;
    public readonly UnexpectedUserAction ActionForUnexpectedDefaultAuthUsers = UnexpectedUserAction.Report;
    public readonly UnexpectedUserAction ActionForUnexpectedOpenIdUsers = UnexpectedUserAction.Report;
    //Site missing users needing provisioning
    public readonly MissingUserAction ActionForMissingSamlUsers = MissingUserAction.Report;
    public readonly MissingUserAction ActionForMissingDefaultAuthUsers = MissingUserAction.Report;
    public readonly MissingUserAction ActionForMissingOpenIdUsers = MissingUserAction.Report;
    //Site existing users needing modification
    public readonly ExistingUserAction ActionForExistingSamlUsers = ExistingUserAction.Report;
    public readonly ExistingUserAction ActionForExistingDefaultAuthUsers = ExistingUserAction.Report;
    public readonly ExistingUserAction ActionForExistingOpenIdUsers = ExistingUserAction.Report;

    //Group membership
    public readonly UnexpectedGroupMemberAction ActionForUnexpectedGroupMembers = UnexpectedGroupMemberAction.Report;
    public readonly MissingGroupMemberAction ActionForMissingGroupMembers = MissingGroupMemberAction.Report;

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

        //*************************************************************************************
        //Get instructions about the intended site membership provisioning
        //*************************************************************************************
        var xnodeSiteMembership = xmlConfigTargetSite.SelectSingleNode("//SiteProvisioning/SiteMembership");
        if(xnodeSiteMembership != null)
        {
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
        }
        else
        {
            //A NULL action indicates that should SKIP this section of the provisioning
            this.UsersToProvision = null;
        }

        //*************************************************************************************
        //Groups' memberships
        //*************************************************************************************
        var xnodeGroups = xmlConfigTargetSite.SelectSingleNode("//SiteProvisioning/GroupsMemberships");
        if (xnodeGroups != null)
        {
            this.ActionForMissingGroupMembers = ParseMissingGroupMemberAction(
                                                    xnodeGroups.Attributes[ProvisionUserInstructions.XmlAttribute_MissingGroupMembers].Value);

            this.ActionForUnexpectedGroupMembers = ParseUnexpectedGroupMemberAction(
                                            xnodeGroups.Attributes[ProvisionUserInstructions.XmlAttribute_UnexpectedGroupMembers].Value);

            //Load each group's membership information
            var provisionGroups = new List<ProvisioningGroup>();
            var xnodesGroups = xmlConfigTargetSite.SelectNodes("//SiteProvisioning/GroupsMemberships/GroupMembership");
            foreach (XmlNode xmlThisGroup in xnodesGroups)
            {
                var thisGroup = new ProvisioningGroup(xmlThisGroup);
                provisionGroups.Add(thisGroup);
            }
            this.GroupsToProvision = provisionGroups.AsReadOnly();
        }
        else
        {
            //A NULL action indicates that should SKIP this section of the provisioning
            this.GroupsToProvision = null;
        }

        //*************************************************************************************
        //Content ownership mapping
        //*************************************************************************************
        var xnodeContentOwnership = xmlConfigTargetSite.SelectSingleNode("//SiteProvisioning/ContentOwnership");
        if (xnodeContentOwnership != null)
        {
            var provisionContentOwnwership = new List<ProvisioningChangeContentOwnership>();
            foreach (XmlNode xmlThisChangeOwnership in xnodeContentOwnership)
            {
                var thisChangeOwnership = new ProvisioningChangeContentOwnership(xmlThisChangeOwnership);
                provisionContentOwnwership.Add(thisChangeOwnership);
            }
            this.ContentOwnershipToProvision = provisionContentOwnwership.AsReadOnly();
        }
        else
        {
            //A NULL action indicates that should SKIP this section of the provisioning
            this.ContentOwnershipToProvision = null;
        }

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