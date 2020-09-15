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
        /// Constructor
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <param name="targetGroup"></param>
        public SynchronizeGroupToGroup(string sourceGroup, string targetGroup)
        {
            this.SourceGroupName = sourceGroup;
            this.TargetGroupName = targetGroup;
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
    }
}