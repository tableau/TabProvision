using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


internal class ProvisioningUser
{
    public readonly string UserAuthentication;
    public readonly SiteUserAuth UserAuthenticationParsed;
    //public readonly SiteUserRole UserRoleParsed;
    public readonly string  UserRole;
    public readonly string UserName;
    public readonly int RoleRank;
    public readonly string SourceGroup;

    /// <summary>
    /// Explictly passed in value
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuthentication"></param>
    public ProvisioningUser(string userName, string userRole, string userAuthentication, string sourceGroup)
    {
        this.UserName = userName;
        this.UserRole = userRole;
        this.UserAuthentication = userAuthentication;
        this.UserAuthenticationParsed = SiteUser.ParseUserAuthentication(this.UserAuthentication);
        this.RoleRank = CalculateRoleRank(userRole);
        this.SourceGroup = sourceGroup;
    }


    public const int RoleRank_Unlicensed = 0;
    public const int RoleRank_Viewer = 10;
    public const int RoleRank_Explorer = 20;
    public const int RoleRank_ExplorerCanPublish = 30;
    public const int RoleRank_Creator = 40;
    public const int RoleRank_SiteAdministratorExplorer = 50;
    public const int RoleRank_SiteAdministratorCreator = 100;
    public const string RoleText_SiteAdministratorCreator = "SiteAdministratorCreator";

    /// <summary>
    /// True of the combination of 2 roles is a "Creator" + "Administrator"
    /// </summary>
    /// <param name="userRoleScore1"></param>
    /// <param name="userRoleScore2"></param>
    /// <returns></returns>
    public static bool IsCombinedUserRoleCreatorAdministrator(int userRoleScore1, int userRoleScore2)
    {
        //==================================================================================
        //If either role is a Admin-Creator, then the user is an admin creator
        //==================================================================================
        if ((userRoleScore1 == RoleRank_SiteAdministratorCreator) || (userRoleScore2 == RoleRank_SiteAdministratorCreator))
        {
            return true;
        }

        //=========================================================================================
        //If 1 role is Creator, and the other an ExplorerAdmin, then the combined role is Creator Admin
        //=========================================================================================
        if ((userRoleScore1 == RoleRank_SiteAdministratorExplorer) && (userRoleScore2 == RoleRank_Creator))
        {
            return true;
        }

        if ((userRoleScore2 == RoleRank_SiteAdministratorExplorer) && (userRoleScore1 == RoleRank_Creator))
        {
            return true;
        }


        //The conbination of these 2 roles is NOT Creator+Admin
        return false;
    }

    /// <summary>
    /// When a user is in multiple groups, we need to choose the highest rank for their seat provisioning.
    /// This calculation helps us do that
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    private static int CalculateRoleRank(string role)
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

        IwsDiagnostics.Assert(false, "813-549: Unknown role " + role);
        throw new Exception("813-549: Unknown role " + role);

    }

    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningUser(XmlNode xmlNode, string overrideSourceGroup)
    {
        this.UserAuthentication = xmlNode.Attributes["auth"].Value;
        this.UserRole = xmlNode.Attributes["role"].Value;
        this.UserName = xmlNode.Attributes["name"].Value;

        if(!string.IsNullOrEmpty(overrideSourceGroup))
        {
            this.SourceGroup = overrideSourceGroup;
        }
        else
        {
            var xAttributeSourceGroup = xmlNode.Attributes["sourceGroup"];
            if(xAttributeSourceGroup != null)
            {
                this.SourceGroup = xAttributeSourceGroup.Value;
            }
        }


        this.UserAuthenticationParsed = SiteUser.ParseUserAuthentication(this.UserAuthentication);
    }

    /// <summary>
    /// Write out XML for the object
    /// </summary>
    /// <param name="xmlWriter"></param>
    internal void WriteAsXml(XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("User");
        xmlWriter.WriteAttributeString("name", this.UserName);
        xmlWriter.WriteAttributeString("role", this.UserRole);
        xmlWriter.WriteAttributeString("auth", this.UserAuthentication);

        if(!string.IsNullOrEmpty(this.SourceGroup))
        {
            xmlWriter.WriteAttributeString("sourceGroup", this.SourceGroup);
        }
        xmlWriter.WriteEndElement();

    }
}