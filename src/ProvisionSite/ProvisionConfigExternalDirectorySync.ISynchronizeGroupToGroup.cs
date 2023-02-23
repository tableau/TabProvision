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
    public interface ISynchronizeGroupToGroup
    {
        /// <summary>
        /// The source name (or pattern) in Azure AD
        /// </summary>
        string SourceGroupName { get; }

        /// <summary>
        /// Generate what the Tableau group name should be.  Often this is an explictly stated target group name
        /// </summary>
        /// <param name="sourceGroup"></param>
        /// <returns></returns>
        string GenerateTargetGroupName(string sourceGroup);

        /// <summary>
        /// Pattern matching to apply to find the group in the source directory
        /// </summary>
        NamePatternMatch NamePatternMatch { get; }


        /// <summary>
        /// A simple "contains" filter to apply in addition to the pattern match
        /// </summary>
        string FilterSourceGroupNameContains { get; }


        /// <summary>
        /// If NOT NULL, then this is a group name that we always expect to have 
        /// on the target (i.e. Tableau) side.  If there are 0 sourcs members found
        /// for this group, then we will want to modify the Tableau group to relect this
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        string RequiredTargetGroupNameOrNull { get; }


        /// <summary>
        /// Instructions for what to do about the Grant License mode of the Tableau site Group
        /// </summary>
        ProvisioningGroup.GrantLicenseMode GrantLicenseInstructions { get; }

        /// <summary>
        /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "ExplorerCanPublish", "Viewer", types of admins, ...
        /// This may be NULL if there is no Grant License instruction
        /// </summary>
        string GrantLicenseRole { get; }

    }

}