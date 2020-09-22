using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;

/// <summary>
/// Provision a site
/// </summary>
internal partial class AzureDownload
{
    /// <summary>
    /// Generate the filtering command based on the specified pattern match
    /// </summary>
    /// <param name="namePattern"></param>
    /// <param name="attribute"></param>
    /// <param name="matchValue"></param>
    /// <returns></returns>
    private static string GenerateAzureMatchCommand(ProvisionConfigExternalDirectorySync.NamePatternMatch namePattern, string attribute, string matchValue)
    {
        //See Microsoft docs: https://docs.microsoft.com/en-us/graph/query-parameters
        switch (namePattern)
        {
            //Equality
            case ProvisionConfigExternalDirectorySync.NamePatternMatch.Equals:
                return attribute + " eq '" + matchValue + "'";
            //Starts with
            case ProvisionConfigExternalDirectorySync.NamePatternMatch.StartsWith:
                return "startswith(" + attribute + ", '" + matchValue + "')";
            //Unknown...
            default:
                IwsDiagnostics.Assert(false, "914-100: Unknown name pattern match");
                throw new Exception("914-100: Unknown name pattern match");
        }
    }

}
