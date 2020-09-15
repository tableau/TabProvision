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
    private const string NamePatternMatch_Equals = "equals";
    private const string NamePatternMatch_StartsWith = "startswith";

    /// <summary>
    /// Parse the name pattern match
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static NamePatternMatch ParseNamePatternMatch(string text)
    {
        text = text.ToLower();
        if(text == NamePatternMatch_Equals) { return NamePatternMatch.Equals; }
        if (text == NamePatternMatch_StartsWith) { return NamePatternMatch.StartsWith; }

        throw new Exception("914-1254: Unknown name pattern match " + text);
    }

    /// <summary>
    /// Types of name pattern matches
    /// </summary>
    public enum NamePatternMatch
    {
        Equals,
        StartsWith
    }

}