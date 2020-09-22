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
    /// A mapping of MULTIPLE Groups (e.g. e.g. AzureAD groups) to Tableau Site's Groups
    /// Allows using a pattern matching rule to grab multiple groups
    /// </summary>
    public class SynchronizePatternMatchingGroupToGroup : ISynchronizeGroupToGroup
    {
        public readonly string SourceGroupName;
        public readonly NamePatternMatch NamePatternMatch;

        /// <summary>
        /// Instructions for what to do about the Grant License mode of the Tableau site Group
        /// </summary>
        public readonly ProvisioningGroup.GrantLicenseMode GrantLicenseInstructions;

        /// <summary>
        /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "Viewer", types of admins, ...
        /// </summary>
        public readonly string GrantLicenseRole;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <param name="namePatternMatch">Pattern match to find matching groups in the source directory</param>
        public SynchronizePatternMatchingGroupToGroup(
            string sourceGroup,
            NamePatternMatch namePatternMatch)
        {
            this.SourceGroupName = sourceGroup;
            this.NamePatternMatch = namePatternMatch;
        }

        /// <summary>
        /// Constructor: From XML
        /// </summary>
        /// <param name="xmlNode"></param>
        public SynchronizePatternMatchingGroupToGroup(XmlNode xmlNode)
        {
            this.SourceGroupName = xmlNode.Attributes[ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup.XmlAttribute_SoruceGroup].Value;

            this.NamePatternMatch =
                ProvisionConfigExternalDirectorySync.ParseNamePatternMatch(
                    XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_SourceGroupMatch, NamePatternMatch_Equals));

            //Read in the grant license attributes
            ProvisioningGroup.ReadGrantLicenseXmlAttributes(
                xmlNode,
                this.SourceGroupName,
                out this.GrantLicenseInstructions,
                out this.GrantLicenseRole);
        }


        /// <summary>
        /// Name of the source group
        /// </summary>
        string ISynchronizeGroupToGroup.SourceGroupName
        {
            get
            {
                return this.SourceGroupName;
            }
        }

        /// <summary>
        /// Pattern matching rule for finding matching Groups in the source directory
        /// </summary>
        NamePatternMatch ISynchronizeGroupToGroup.NamePatternMatch
        {
            get
            {
                return this.NamePatternMatch;
            }
        }

        /// <summary>
        /// The Target (Tableau) group has the same name as the Source group
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <returns></returns>
        string ISynchronizeGroupToGroup.GenerateTargetGroupName(string sourceGroup)
        {
            return sourceGroup;
        }

        /// <summary>
        /// There is to explicity known Target Groups
        /// </summary>
        string ISynchronizeGroupToGroup.RequiredTargetGroupNameOrNull
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Does this Group support Grant License on Sign In? (if so, what role?)
        /// </summary>
        string ISynchronizeGroupToGroup.GrantLicenseRole
        {
            get
            {
                return this.GrantLicenseRole;
            }
        }

        /// <summary>
        /// Does this Group support Grant License on Sign In?
        /// </summary>
        ProvisioningGroup.GrantLicenseMode ISynchronizeGroupToGroup.GrantLicenseInstructions
        {
            get
            {
                return this.GrantLicenseInstructions;
            }
        }

    }

}