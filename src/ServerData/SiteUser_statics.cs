using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

partial class SiteUser : IHasSiteItemId
{
    /// <summary>
    /// REST API attribute values
    /// </summary>
    public const string Role_Unlicensed = "Unlicensed";

    /// <summary>
    /// Parse into known roles
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static SiteUserAuth ParseUserAuthentication(string authText)
    {
        if(string.IsNullOrEmpty(authText))
        {
            return SiteUserAuth.Unknown;
        }

        authText = authText.ToLower();
        switch (authText)
        {
            case "serverdefault":
                return SiteUserAuth.ServerDefault;

            case "saml":
                return SiteUserAuth.SAML;

            case "openid":
                return SiteUserAuth.OpenID;

            case "tableauidwithmfa":
                return SiteUserAuth.TableauIDWithMFA;

            default:
                return SiteUserAuth.Unknown;
        }
    }

    /// <summary>
    /// Turn a parsed role into an attribute string
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static string UserAuthenticationToString(SiteUserAuth userAuth)
    {
        switch(userAuth)
        {
            case SiteUserAuth.ServerDefault:
                return "ServerDefault";

            case SiteUserAuth.SAML:
                return "SAML";

            case SiteUserAuth.OpenID:
                return "OpenId";

            case SiteUserAuth.TableauIDWithMFA:
                return "TableauIDWithMFA";

            case SiteUserAuth.Unknown:
                return "*Unknown*";

            default:
                IwsDiagnostics.Assert(false, "1018-100: Unrecognized user auth type");
                return "*Unrecognized*";
        }
    }

    /// <summary>
    /// Parse into known roles
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static SiteUserRole ParseUserRole(string roleName)
    {
        roleName = roleName.ToLower();
        switch(roleName)
        {
            case "serveradministrator":
                return SiteUserRole.ServerAdministrator;
            case "creator":
                return SiteUserRole.Creator;
            case "siteadministratorcreator":
                return SiteUserRole.SiteAdministratorCreator;
            case "siteadministratorexplorer":
                return SiteUserRole.SiteAdministratorExplorer;
            case "explorer":
                return SiteUserRole.Explorer;
            case "explorercanpublish":
                return SiteUserRole.ExplorerCanPublish;
            case "siteadministrator": //Legacy
            case "interactor":  //[2019-11-02] Legacy role (still shows up in site user data)
            case "publisher":   //[2019-11-02] Legacy role (still shows up in site user data)
                return SiteUserRole.Explorer;

            case "viewer":
                return SiteUserRole.Viewer;

            case "unlicensed":
                return SiteUserRole.Unlicensed;

            default:
                return SiteUserRole.Unknown;
        }
    }

    /// <summary>
    /// Parse into known roles
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public static bool ParseIsSiteAdmin(string roleName)
    {
        roleName = roleName.ToLower();
        switch (roleName)
        {
            case "serveradministrator":
            case "siteadministratorcreator":
            case "siteadministratorexplorer":
                return true;

            default:
                return false;
        }
    }


    /// <summary>
    /// Look through a set of users for a user with a specific name
    /// </summary>
    /// <param name="siteUsers"></param>
    /// <param name="findName"></param>
    /// <param name="compareMode"></param>
    /// <returns> NULL = No matching user found.  Otherwise returns the user with the matching name
    /// </returns>
    public static SiteUser FindUserWithName(IEnumerable<SiteUser> siteUsers, string findName, StringComparison compareMode = StringComparison.InvariantCultureIgnoreCase)
    {
        foreach(var thisUser in siteUsers)
        {
            //If its a match return the user
            if(string.Compare(thisUser.Name, findName, compareMode) == 0)
            {
                return thisUser;
            }
        }

        return null; //no found
    }
}
