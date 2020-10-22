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
    /// A mapping of source Group (e.g. AzureAD group) to Tableau Online/Server role
    /// </summary>
    public partial class SynchronizeGroupToRole
    {

        public readonly string SourceGroupName;
        public readonly string TableauRole;
        public readonly string AuthenticationModel;

        public readonly bool AllowPromotedRoleForMembers;
        /// <summary>
        /// Are we doing a name pattern match
        /// </summary>
        public readonly NamePatternMatch NamePatternMatch;
        public readonly string FilterSourceGroupNameContains = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <param name="tableauRole"></param>
        public SynchronizeGroupToRole(string sourceGroup, string tableauRole, string authModel, bool allowPromotedRole, NamePatternMatch namePatternMatch)
        {
            this.SourceGroupName = sourceGroup;
            this.TableauRole = tableauRole;
            this.AuthenticationModel = authModel;
            this.NamePatternMatch = namePatternMatch;

            // TRUE: It is not unexpected to find that the user has an acutal role > than this specified role (useful for Grant License on Sign In scenarios)
            // FALSE: It is unexpected to find the user with a role that differs from this specified role
            this.AllowPromotedRoleForMembers = allowPromotedRole;
        }


        /// <summary>
        /// Constructor: From XML
        /// </summary>
        /// <param name="xmlNode"></param>
        public SynchronizeGroupToRole(XmlNode xmlNode)
        {
            this.SourceGroupName = xmlNode.Attributes[XmlAttribute_SourceGroup].Value;
            this.TableauRole = xmlNode.Attributes[XmlAttribute_TargetRole].Value;
            this.AuthenticationModel = xmlNode.Attributes[ProvisioningUser.XmlAttribute_Auth].Value; 
            this.NamePatternMatch = ParseNamePatternMatch(
                XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_SourceGroupMatch, NamePatternMatch_Equals));
            //An optional filter to apply
            this.FilterSourceGroupNameContains = XmlHelper.SafeParseXmlAttribute(xmlNode, XmlAttribute_FilterSourceGroupContains, "");


            // TRUE: It is not unexpected to find that the user has an acutal role > than this specified role (useful for Grant License on Sign In scenarios)
            // FALSE: It is unexpected to find the user with a role that differs from this specified role
            this.AllowPromotedRoleForMembers = XmlHelper.SafeParseXmlAttribute_Boolean(
                xmlNode, ProvisioningUser.XmlAttribute_AllowPromotedRole, false); ;

        }
    }
}