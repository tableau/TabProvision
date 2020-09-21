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
    public const string XmlAttribute_GrantLicenseSiteRole = "grantLicenseSiteRole";

    /// <summary>
    /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "Viewer", types of admins, ...
    /// </summary>
    public readonly string GrantLicenseRole;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningGroup(XmlNode xmlNode)
    {
        this.GroupName = xmlNode.Attributes["name"].Value;

        //Parse the grant license mode
        this.GrantLicenseInstructions = ParseGrantLicenseMode(
            XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_GrantLicenseMode, ""));

        //Store the target role
        this.GrantLicenseRole =
            XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_GrantLicenseSiteRole, null);

        //=======================================================================
        //Sanity check
        //=======================================================================
        if(this.GrantLicenseInstructions == GrantLicenseMode.OnLogin)
        {
            if(string.IsNullOrWhiteSpace(this.GrantLicenseRole))
            {
                throw new Exception("920-1010: grantLicenseRole cannot be blank for group: " + this.GroupName);
            }

            var sanityParseRole = SiteUser.ParseUserRole(this.GrantLicenseRole);
            if(sanityParseRole == SiteUserRole.Unknown)
            {
                throw new Exception("920-1007: Unknown grant license role found for group: " + this.GroupName + "/" + this.GrantLicenseRole);
            }
        }

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
}