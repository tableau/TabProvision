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
    /// What we do with exiting users that need to be updated
    /// </summary>
    internal enum ExistingUserAction
    {
        /// <summary>
        /// Keep the  user 'as is' 
        /// </summary>
        Report,

        /// <summary>
        /// Modify the user to the Tableau site
        /// </summary>
        Modify
    }
}
