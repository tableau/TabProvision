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
    /// <summary>
    /// A mapping of source Group (e.g. AzureAD group) to Tableau Site's Group
    /// </summary>
    public class SynchronizeGroupToGroup : ISynchronizeGroupToGroup
    {
        public readonly string SourceGroupName;
        public readonly string TargetGroupName;

        /// <summary>
        /// Instructions for what to do about the Grant License mode of the Tableau site Group
        /// </summary>
        public readonly ProvisioningGroup.GrantLicenseMode GrantLicenseInstructions;

        /// <summary>
        /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "Viewer", types of admins, ...
        /// </summary>
        public readonly string GrantLicenseRole;


        //XML attributes
        public const string XmlAttribute_SoruceGroup = "sourceGroup";
        public const string XmlAttribute_TargetGroup = "targetGroup";


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <param name="targetGroup"></param>
        public SynchronizeGroupToGroup(string sourceGroup, string targetGroup)
        {
            this.SourceGroupName = sourceGroup;
            this.TargetGroupName = targetGroup;
        }

        /// <summary>
        /// Constructor: From XML
        /// </summary>
        /// <param name="xmlNode"></param>
        public SynchronizeGroupToGroup(XmlNode xmlNode)
        {
            this.SourceGroupName = xmlNode.Attributes[XmlAttribute_SoruceGroup].Value;
            //Note: If there is no target group specified, use the source group name
            this.TargetGroupName = XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_TargetGroup, this.SourceGroupName);


            //Read in the grant license attributes
            ProvisioningGroup.ReadGrantLicenseXmlAttributes(
                xmlNode,
                this.SourceGroupName,
                out this.GrantLicenseInstructions,
                out this.GrantLicenseRole);
        }

        string ISynchronizeGroupToGroup.SourceGroupName
        {
            get
            {
                return this.SourceGroupName;
            }
        }

        NamePatternMatch ISynchronizeGroupToGroup.NamePatternMatch
        {
            get
            {
                return ProvisionConfigExternalDirectorySync.NamePatternMatch.Equals;
            }
        }

        string ISynchronizeGroupToGroup.GenerateTargetGroupName(string sourceGroup)
        {
            return this.TargetGroupName;
        }

        string ISynchronizeGroupToGroup.RequiredTargetGroupNameOrNull
        {
            get
            {
                return this.TargetGroupName;
            }
        }

        string ISynchronizeGroupToGroup.GrantLicenseRole
        {
            get
            {
                return this.GrantLicenseRole;
            }
        }

        ProvisioningGroup.GrantLicenseMode ISynchronizeGroupToGroup.GrantLicenseInstructions
        {
            get
            {
                return this.GrantLicenseInstructions;
            }
        }

    }
}