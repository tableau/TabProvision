using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Defines a Group we want to create in the Tableau site, and who the members are
/// </summary>
internal partial class ProvisioningGroup
{
    /// <summary>
    /// Name of the group in the Tableau site
    /// </summary>
    public readonly string GroupName;
    /// <summary>
    /// Members in the group
    /// </summary>
    public readonly ReadOnlyCollection<string> Members;

    /// <summary>
    /// Instructions for what to do about the Grant License mode of the Tableau site Group
    /// </summary>
    public readonly GrantLicenseMode GrantLicenseInstructions;

    public const string XmlAttribute_GrantLicenseMode = "grantLicenseMode";
    public const string XmlAttribute_GrantLicenseMinimumSiteRole = "grantLicenseMinimumSiteRole";

    /// <summary>
    /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "ExplorerCanPublish", "Viewer", types of admins, ...
    /// </summary>
    public readonly string GrantLicenseRole;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningGroup(XmlNode xmlNode)
    {
        this.GroupName = xmlNode.Attributes["name"].Value;

        //Read in the grant license attributes
        ReadGrantLicenseXmlAttributes(
            xmlNode,
            this.GroupName,
            out this.GrantLicenseInstructions,
            out this.GrantLicenseRole);

        //==========================================================================
        ///Load all the members of the group
        //==========================================================================
        var usersInGroup = new List<string>();
        var xNodesMembers = xmlNode.SelectNodes(".//GroupMember");
        foreach(XmlNode xmlGroupMember in xNodesMembers)
        {
            usersInGroup.Add(xmlGroupMember.Attributes["name"].Value);
        }
        this.Members = usersInGroup.AsReadOnly();
    }


    /// <summary>
    /// Parse the grant license values
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="groupName"></param>
    /// <param name="grantLicenseMode"></param>
    /// <param name="grantLicenseRole"></param>
    public static void ReadGrantLicenseXmlAttributes(XmlNode xmlNode, string groupName, out GrantLicenseMode grantLicenseMode, out string grantLicenseRole)
    {
        //Parse the grant license mode
        grantLicenseMode = ParseGrantLicenseMode(
            XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_GrantLicenseMode, ""));

        //Store the target role
        grantLicenseRole =
            XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_GrantLicenseMinimumSiteRole, null);

        //=======================================================================
        //Sanity check
        //=======================================================================
        if (grantLicenseMode == GrantLicenseMode.OnLogin)
        {
            if (string.IsNullOrWhiteSpace(grantLicenseRole))
            {
                throw new Exception("920-1010: grantLicenseMinimumSiteRole cannot be blank for group: " + groupName);
            }

            var sanityParseRole = SiteUser.ParseUserRole(grantLicenseRole);
            if (sanityParseRole == SiteUserRole.Unknown)
            {
                throw new Exception("920-1007: Unknown grant license role found for group: " + groupName + "/" + grantLicenseRole);
            }
        }

    }


}