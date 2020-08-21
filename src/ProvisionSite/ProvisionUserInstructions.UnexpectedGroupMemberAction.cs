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
    /// What we do with group members who are there, but are unexpected
    /// </summary>
    internal enum UnexpectedGroupMemberAction
    {
        /// <summary>
        /// Keep the unexpected user 'as is' 
        /// </summary>
        Report,

        /// <summary>
        /// Delete the member from the group
        /// </summary>
        Delete
    }
}
