using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/// <summary>
/// Instructions for provisioning a single user
/// </summary>
internal partial class ProvisioningUser
{
    public readonly string UserAuthentication;
    public readonly SiteUserAuth UserAuthenticationParsed;
    //public readonly SiteUserRole UserRoleParsed;
    public readonly string  UserRole;
    public readonly string UserName;
    public readonly int RoleRank = RoleRank_Error; //Start with an error value (should get set in the constructor)
    public readonly string SourceGroup;

    /// <summary>
    /// TRUE: It is not unexpected to find that the user has an acutal role > than this specified role (useful for Grant License on Sign In scenarios)
    /// FALSE: It is unexpected to find the user with a role that differs from this specified role
    /// </summary>
    public readonly bool AllowPromotedRole;


    /// <summary>
    /// Explictly passed in value
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuthentication"></param>
    public ProvisioningUser(string userName, string userRole, string userAuthentication, string sourceGroup, bool allowPromotedRole)
    {
        this.UserName = userName;
        this.UserRole = userRole;
        this.UserAuthentication = userAuthentication;
        this.UserAuthenticationParsed = SiteUser.ParseUserAuthentication(this.UserAuthentication);
        this.RoleRank = CalculateRoleRank(userRole);
        this.SourceGroup = sourceGroup;
        this.AllowPromotedRole = allowPromotedRole; 
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningUser(XmlNode xmlNode, string overrideSourceGroup)
    {
        this.UserAuthentication = xmlNode.Attributes[XmlAttribute_Auth].Value;
        this.UserRole = xmlNode.Attributes[XmlAttribute_Role].Value;
        this.UserName = xmlNode.Attributes[XmlAttribute_Name].Value;

        this.RoleRank = CalculateRoleRank(this.UserRole);


        /// <summary>
        /// TRUE: It is not unexpected to find that the user has an acutal role > than this specified role (useful for Grant License on Sign In scenarios)
        /// FALSE: It is unexpected to find the user with a role that differs from this specified role
        /// </summary>
        this.AllowPromotedRole =  XmlHelper.ReadBooleanAttribute(xmlNode, XmlAttribute_AllowPromotedRole, false);

        //Source group information (useful for understanding where this role definition came from)
        if (!string.IsNullOrEmpty(overrideSourceGroup))
        {
            this.SourceGroup = overrideSourceGroup;
        }
        else
        {
            var xAttributeSourceGroup = xmlNode.Attributes[XmlAttribute_SourceGroup];
            if (xAttributeSourceGroup != null)
            {
                this.SourceGroup = xAttributeSourceGroup.Value;
            }
        }


        this.UserAuthenticationParsed = SiteUser.ParseUserAuthentication(this.UserAuthentication);
    }


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
    /// Write out XML for the object
    /// </summary>
    /// <param name="xmlWriter"></param>
    internal void WriteAsXml(XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("User");
        xmlWriter.WriteAttributeString(XmlAttribute_Name, this.UserName);
        xmlWriter.WriteAttributeString(XmlAttribute_Role, this.UserRole);
        xmlWriter.WriteAttributeString(XmlAttribute_Auth, this.UserAuthentication);
        XmlHelper.WriteBooleanAttribute(xmlWriter, XmlAttribute_AllowPromotedRole, this.AllowPromotedRole);

        if (!string.IsNullOrEmpty(this.SourceGroup))
        {
            xmlWriter.WriteAttributeString(XmlAttribute_SourceGroup, this.SourceGroup);
        }
        xmlWriter.WriteEndElement();
    }
}