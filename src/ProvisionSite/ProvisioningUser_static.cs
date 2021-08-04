using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

internal partial class ProvisioningUser
{

    public const int RoleRank_Error = -200;
    public const int RoleRank_Unlicensed = 0;
    public const int RoleRank_Viewer = 10;
    public const int RoleRank_Explorer = 20;
    public const int RoleRank_ExplorerCanPublish = 30;
    public const int RoleRank_Creator = 40;
    public const int RoleRank_SiteAdministratorExplorer = 50;
    public const int RoleRank_SiteAdministratorCreator = 100;
    public const int RoleRank_ServerAdministrator = 500;
    public const string RoleText_SiteAdministratorCreator = "SiteAdministratorCreator";

    public const string XmlAttribute_Role = "role";
    public const string XmlAttribute_Name = "name";
    public const string XmlAttribute_SourceGroup = "sourceGroup";
    public const string XmlAttribute_Auth = "auth";
    /// <summary>
    /// TRUE: It is not unexpected to find that the user has an acutal role > than this specified role (useful for Grant License on Sign In scenarios)
    /// FALSE: It is unexpected to find the user with a role that differs from this specified role
    /// </summary>
    public const string XmlAttribute_AllowPromotedRole = "allowPromotedRole";

    /// <summary>
    /// When a user is in multiple groups, we need to choose the highest rank for their seat provisioning.
    /// This calculation helps us do that
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static int CalculateRoleRank(string role)
    {
        string cannonicalRole = role.ToLower();

        if (cannonicalRole == "unlicensed")                return RoleRank_Unlicensed;
        if (cannonicalRole == "viewer")                    return RoleRank_Viewer;
        if (cannonicalRole == "explorer")                  return RoleRank_Explorer;
        if (cannonicalRole == "explorercanpublish")        return RoleRank_ExplorerCanPublish;
        if (cannonicalRole == "creator")                   return RoleRank_Creator;
        if (cannonicalRole == "siteadministratorexplorer") return RoleRank_SiteAdministratorExplorer;
        if (cannonicalRole == "siteadministrator")         return RoleRank_SiteAdministratorExplorer;
        if (cannonicalRole == "siteadministratorcreator")  return RoleRank_SiteAdministratorCreator;
        if (cannonicalRole == "serveradministrator")       return RoleRank_ServerAdministrator;

        IwsDiagnostics.Assert(false, "813-549: Unknown role " + role);
        throw new Exception("813-549: Unknown role " + role);
    }
}