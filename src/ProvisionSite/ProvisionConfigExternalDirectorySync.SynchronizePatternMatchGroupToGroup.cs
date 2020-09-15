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
    }

}