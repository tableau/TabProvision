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
        /// If TRUE, the source group is a RegEx expression
        /// </summary>
        public readonly NamePatternMatch NamePatternMatch;

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
    }
}