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
    /// What we do with exiting users that are not in our provision list
    /// </summary>
    internal enum MissingUserAction
    {
        /// <summary>
        /// Keep the unexpected user 'as is' 
        /// </summary>
        Report,

        /// <summary>
        /// Add the user to the Tableau site
        /// </summary>
        Add
    }
}
