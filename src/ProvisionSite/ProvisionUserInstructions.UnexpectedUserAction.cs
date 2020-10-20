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
    internal enum UnexpectedUserAction
    {
        /// <summary>
        /// Keep the unexpected user 'as is' 
        /// </summary>
        Report,

        /// <summary>
        /// Change the unexpected user to be unlicensed
        /// </summary>
        Unlicense,


        /// <summary>
        /// Try to DELETE the unexpected user
        /// </summary>
        Delete
    }
}