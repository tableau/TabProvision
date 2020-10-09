using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Config that holds the definition of groups to synchronize (we look these up in AzureAD, Active Directory, LDAP, etc)
/// </summary>
internal partial class ProvisionConfigExternalDirectorySync
{
    public readonly UserEmailMapping EmailMapping;

    /// <summary>
    /// Groups we are using for user roles
    /// </summary>
    public readonly ReadOnlyCollection<SynchronizeGroupToRole> GroupsToRolesSyncList;

    /// <summary>
    /// Groups we are mirroring between the external directory and the Tableau site
    /// </summary>
    public readonly ReadOnlyCollection<ISynchronizeGroupToGroup> GroupsToGroupsSyncList;

    //Explicit list of user/role overrides to perform that supercedes any data we find in the external user directories
    public readonly ReadOnlyCollection<ProvisioningUser> UserRolesOverrideList;

    //Behavioral instructions we want to load
    //Unexpected users
    public readonly ProvisionUserInstructions.UnexpectedUserAction ActionForUnexpectedSamlUsers;
    public readonly ProvisionUserInstructions.UnexpectedUserAction ActionForUnexpectedDefaultAuthUsers;
    public readonly ProvisionUserInstructions.UnexpectedUserAction ActionForUnexpectedOpenIdUsers;
    //Missing users
    public readonly ProvisionUserInstructions.MissingUserAction ActionForMissingSamlUsers;
    public readonly ProvisionUserInstructions.MissingUserAction ActionForMissingDefaultAuthUsers;
    public readonly ProvisionUserInstructions.MissingUserAction ActionForMissingOpenIdUsers;
    //Existing users needing modification
    public readonly ProvisionUserInstructions.ExistingUserAction ActionForExistingSamlUsers;
    public readonly ProvisionUserInstructions.ExistingUserAction ActionForExistingDefaultAuthUsers;
    public readonly ProvisionUserInstructions.ExistingUserAction ActionForExistingOpenIdUsers;
    //Group memberships
    public readonly ProvisionUserInstructions.UnexpectedGroupMemberAction ActionForGroupUnexpectedMembers;
    public readonly ProvisionUserInstructions.MissingGroupMemberAction ActionForGroupMisingMembers;

    private const string XmlAttribute_UserEmailMapping = "userEmailMapping";
    private const string XmlAttribute_SourceGroup = "sourceGroup";
    private const string XmlAttribute_TargetRole = "targetRole";
    private const string XmlAttribute_SourceGroupMatch = "sourceGroupMatch";

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="filePathConfig"></param>
    public ProvisionConfigExternalDirectorySync(string filePathConfig)
    {
        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfig = new System.Xml.XmlDocument();
        xmlConfig.Load(filePathConfig);

        //-------------------------------------------------------------------------------
        //Get any special instructions for email mapping
        //-------------------------------------------------------------------------------
        var xnodeHeader = xmlConfig.SelectSingleNode("//SynchronizeConfiguration");
        this.EmailMapping = ParseUserEmailMapping(
            XmlHelper.SafeParseXmlAttribute(xnodeHeader, XmlAttribute_UserEmailMapping, ""));



        var xnodeRoleSyncHeader = xmlConfig.SelectSingleNode("//SynchronizeConfiguration/SynchronizeRoles");
        //-------------------------------------------------------------------------------
        //Get instructions about the intended site membership provisioning
        //-------------------------------------------------------------------------------
        //UNEXPECTED USERS
        //1.
        this.ActionForUnexpectedSamlUsers = ProvisionUserInstructions.ParseUnexpectedUserActionFromAttribute(
            xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authSamlUnexpectedUsers);
        //2.
        this.ActionForUnexpectedDefaultAuthUsers = ProvisionUserInstructions.ParseUnexpectedUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authDefaultUnexpectedUsers);
        //3.
        this.ActionForUnexpectedOpenIdUsers = ProvisionUserInstructions.ParseUnexpectedUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authOpenIdUnexpectedUsers);

        //MISSING USERS
        //1.
        this.ActionForMissingSamlUsers = ProvisionUserInstructions.ParseMissingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authSamlMissingUsers);
        //2.
        this.ActionForMissingDefaultAuthUsers = ProvisionUserInstructions.ParseMissingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authDefaultMissingUsers);
        //3.
        this.ActionForMissingOpenIdUsers = ProvisionUserInstructions.ParseMissingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authOpenIdMissingUsers);

        //EXISTING USERS
        //1.
        this.ActionForExistingSamlUsers = ProvisionUserInstructions.ParseExistingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authSamlExistingUsers);
        //2.
        this.ActionForExistingDefaultAuthUsers = ProvisionUserInstructions.ParseExistingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authDefaultExistingUsers);
        //3.
        this.ActionForExistingOpenIdUsers = ProvisionUserInstructions.ParseExistingUserActionFromAttribute(
                                xnodeRoleSyncHeader, ProvisionUserInstructions.XmlAttribute_authOpenIdExistingUsers);


        //-------------------------------------------------------------------------------
        //Get instructions about the intended group membership provisioning
        //-------------------------------------------------------------------------------
        var xnodeGroupsSyncHeader = xmlConfig.SelectSingleNode("//SynchronizeConfiguration/SynchronizeGroups");
        this.ActionForGroupMisingMembers = ProvisionUserInstructions.ParseMissingGroupMemberAction(
                        xnodeGroupsSyncHeader.Attributes[ProvisionUserInstructions.XmlAttribute_MissingGroupMembers].Value);

        this.ActionForGroupUnexpectedMembers = ProvisionUserInstructions.ParseUnexpectedGroupMemberAction(
                        xnodeGroupsSyncHeader.Attributes[ProvisionUserInstructions.XmlAttribute_UnexpectedGroupMembers].Value);


        //---------------------------------------------------------------------------------------------
        //Load the list User/Role mapping groups we want to look up in the external directory
        //---------------------------------------------------------------------------------------------
        this.GroupsToRolesSyncList = ParseGroupsToRoles(xmlConfig).AsReadOnly();


        //--------------------------------------------------------------------------------------
        //Load any user/role overrides that may be defined for the site
        //--------------------------------------------------------------------------------------
        this.UserRolesOverrideList = ProvisionUserInstructions.ParseUsers(
            xmlConfig,
            "//SynchronizeConfiguration/SynchronizeRoles/SiteMembershipOverrides/User",
            "**OVERRIDE**").AsReadOnly();

        //---------------------------------------------------------------------------------------------
        //Load the list of group/group mappings we want to look up in the external directory
        //---------------------------------------------------------------------------------------------
        List<ISynchronizeGroupToGroup> groupsToSync = new List<ISynchronizeGroupToGroup>();
        //1. Load and add explicit group/group mappings
        groupsToSync.AddRange(ParseGroupsToGroups(xmlConfig));
        //2. Load and add pattern matching group/group mappings
        groupsToSync.AddRange(ParsePatternMatchGroupsToGroups(xmlConfig));

        this.GroupsToGroupsSyncList = groupsToSync.AsReadOnly();

    }

    /// <summary>
    /// Parse the Group to Tableau Role mappings from the XML
    /// </summary>
    /// <param name="xmlConfig"></param>
    /// <returns></returns>
    private List<SynchronizeGroupToRole> ParseGroupsToRoles(XmlDocument xmlConfig)
    {
        var listOut = new List<SynchronizeGroupToRole>();

        var xNodesGroupToRole = xmlConfig.SelectNodes("//SynchronizeConfiguration/SynchronizeRoles/SynchronizeRole");
        foreach(XmlNode thisXmlNode in xNodesGroupToRole)
        {
            var thisMapping = new SynchronizeGroupToRole(thisXmlNode);
            listOut.Add(thisMapping);
        }

        return listOut;
    }

    /// <summary>
    /// Parse the Group to Tableau Site Group mappings from the XML
    /// </summary>
    /// <param name="xmlConfig"></param>
    /// <returns></returns>
    private List<SynchronizeGroupToGroup> ParseGroupsToGroups(XmlDocument xmlConfig)
    {
        var listOut = new List<SynchronizeGroupToGroup>();

        //=================================================================================
        //Get the explicit group/group mapping
        //=================================================================================
        var xNodesGroup = xmlConfig.SelectNodes("//SynchronizeConfiguration/SynchronizeGroups/SynchronizeGroup");
        foreach (XmlNode thisXmlNode in xNodesGroup)
        {
            var thisMapping = new SynchronizeGroupToGroup(thisXmlNode);
            listOut.Add(thisMapping);
        }
            return listOut;
    }

    /// <summary>
    /// Parse the Pattern matching Group to Tableau Site Group mappings from the XML
    /// </summary>
    /// <param name="xmlConfig"></param>
    /// <returns></returns>
    private List<SynchronizePatternMatchingGroupToGroup> ParsePatternMatchGroupsToGroups(XmlDocument xmlConfig)
    {
        var listOut = new List<SynchronizePatternMatchingGroupToGroup>();

        //=================================================================================
        //Get the explicit group/group mapping
        //=================================================================================
        var xNodesGroup = xmlConfig.SelectNodes("//SynchronizeConfiguration/SynchronizeGroups/SynchronizeMatchedGroup");
        foreach (XmlNode thisXmlNode in xNodesGroup)
        {
            var thisMapping = new SynchronizePatternMatchingGroupToGroup(thisXmlNode);
            listOut.Add(thisMapping);
        }
        return listOut;
    }
}