using System;

/// <summary>
/// Config that holds the definition of groups to synchronize (we look these up in AzureAD, Active Directory, LDAP, etc)
/// </summary>
internal partial class ProvisionConfigExternalDirectorySync
{
    private const string UserEmailMapping_UserPrincipalName = "UserPrincipalName";
    private const string UserEmailMapping_UserMail = "UserMail";
    private const string UserEmailMapping_UserMailNickname = "UserMailNickname";
    private const string UserEmailMapping_PreferAzureProxyEmail = "PreferAzureProxyPrimaryEmail";


    /// <summary>
    /// Parse the mapping instructions for finding the users email
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static UserEmailMapping ParseUserEmailMapping(string text)
    {
        if(string.IsNullOrEmpty(text))
        {
            return UserEmailMapping.UserPrincipalName;
        }

        if(string.Compare(text, UserEmailMapping_UserPrincipalName, true) == 0) 
            { return UserEmailMapping.UserPrincipalName; }

        if (string.Compare(text, UserEmailMapping_UserMail, true) == 0)
            { return UserEmailMapping.Mail; }

        if (string.Compare(text, UserEmailMapping_UserMailNickname, true) == 0)
            { return UserEmailMapping.MailNickname; }

        if (string.Compare(text, UserEmailMapping_PreferAzureProxyEmail, true) == 0)
        { return UserEmailMapping.PreferAzureProxyPrimaryEmail; }

        throw new Exception("1009-1200: Unknown email mapping instruction" + text);
    }


    /// <summary>
    /// Types of mappings from the Identity Provider to the email address we need
    /// </summary>
    public enum UserEmailMapping
    {
        /// <summary>
        /// (Default) The user's email is their name in the IdP
        /// </summary>
        UserPrincipalName,

        /// <summary>
        /// If it exists, use Azure's specified Proxy Email instead of user's principal name
        /// </summary>
        PreferAzureProxyPrimaryEmail,

        /// <summary>
        /// Option: The user's mail attribute (email address)
        /// </summary>
        Mail,

        /// <summary>
        /// Option: The user's mail attribute (email address)
        /// </summary>
        MailNickname
    }
}