using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Config that we will pass through the Provision Site process
/// </summary>
internal partial class ProvisionUserInstructions
{

    /// <summary>
    /// What we do with users at are not in the Group we expect them to be
    /// </summary>
    internal enum MissingGroupMemberAction
    {
        /// <summary>
        /// Keep the unexpected user 'as is' 
        /// </summary>
        Report,

        /// <summary>
        /// Add the user to the Tableau site's group
        /// </summary>
        Add
    }
}
