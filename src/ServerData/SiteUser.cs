using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
/// <summary>
/// Information about a User in a Server's site
/// </summary>
partial class SiteUser : IHasSiteItemId
{
    public readonly string Name;
    public readonly string Id;
    public readonly string SiteRole;
    public readonly string SiteAuthentication;
    public readonly DateTime? LastLogin;
    public readonly string LastLoginAsText;
    public readonly SiteUserRole SiteRoleParsed = SiteUserRole.Unknown;
    public readonly SiteUserAuth SiteAuthenticationParsed = SiteUserAuth.Unknown;
    public readonly bool IsSiteAdmin;
    public readonly string FullName;
    public readonly string Email;

    /// <summary>
    /// Any developer/diagnostic notes we want to indicate
    /// </summary>
    public readonly string DeveloperNotes = "";


    /// <summary>
    /// Some server reponses do NOT include a user-id (e.g. Update User requests).  For these we 
    /// do NOT want to try to look up the user-id.
    /// </summary>
    /// <param name="userNode"></param>
    /// <param name="userIdFixed"></param>
    /// <returns></returns>
    public static SiteUser FromUserXMLWithoutUserId(XmlNode userNode, string userIdFixed)
    {
        IwsDiagnostics.Assert(!string.IsNullOrEmpty(userIdFixed), "811-1041: Internal error, expected non-blank user ID");
        return new SiteUser(userNode, userIdFixed, null);
    }

    /// <summary>
    /// Some server reponses do NOT include a user-id (e.g. Update User requests).  For these we 
    /// do NOT want to try to look up the user-id.
    /// </summary>
    /// <param name="userNode"></param>
    /// <param name="userIdFixed"></param>
    /// <returns></returns>
    public static SiteUser FromUserXMLWithoutUserIdOrAuthRole(XmlNode userNode, string userIdFixed, SiteUserAuth authRole)
    {
        IwsDiagnostics.Assert(!string.IsNullOrEmpty(userIdFixed), "1018-104: Internal error, expected non-blank user ID");
        return new SiteUser(userNode, userIdFixed, authRole);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userNode"></param>
    public SiteUser(XmlNode userNode) : this(userNode, null, null)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userNode">XML for user</param>
    /// <param name="userIdFixed">NULL if ID is expected in the XML.  If non-NULL we will use the handed in user-id</param>
    private SiteUser(XmlNode userNode, string userIdFixed, SiteUserAuth? siteUserAuthFixed)
    {
        if (userNode.Name.ToLower() != "user")
        {
            AppDiagnostics.Assert(false, "Not a user");
            throw new Exception("Unexpected content - not user");
        }

        //If we have not been handed a user-id, then it must be in the XML
        if(string.IsNullOrEmpty(userIdFixed))
        {
            this.Id = userNode.Attributes["id"].Value;
        }
        else
        {
            this.Id = userIdFixed;
        }

        this.Name = userNode.Attributes["name"].Value;
        this.SiteRole = userNode.Attributes["siteRole"].Value;

        this.LastLogin = XmlHelper.GetAttributeDateTimeIfExists(userNode, "lastLogin");
        this.LastLoginAsText = XmlHelper.GetAttributeIfExists(userNode, "lastLogin", null);

        //Not all of the REST APIs return full name
        this.FullName = XmlHelper.GetAttributeIfExists(userNode, "fullName", "");

        this.SiteRoleParsed = ParseUserRole(this.SiteRole);
        AppDiagnostics.Assert(this.SiteRoleParsed != SiteUserRole.Unknown, "Unknown user role: " + this.SiteRole);


        //If we were not passed in an explicit user authentication, then it needs to be in the XML
        if(siteUserAuthFixed == null)
        {
            this.SiteAuthentication = userNode.Attributes["authSetting"].Value;

            this.SiteAuthenticationParsed = ParseUserAuthentication(this.SiteAuthentication);
            AppDiagnostics.Assert(this.SiteAuthenticationParsed != SiteUserAuth.Unknown, "Unknown user auth: " + this.SiteAuthenticationParsed);
        }
        else
        {
            //Use the explicitly passed in value
            this.SiteAuthenticationParsed = siteUserAuthFixed.Value;
            this.SiteAuthentication = UserAuthenticationToString(this.SiteAuthenticationParsed);
        }


        this.IsSiteAdmin = ParseIsSiteAdmin(this.SiteRole);

        //=============================================================================
        //[2019-10-30] Currently Query User APIs do not return the user's email.
        //If the User Name is the email (as it is in Tableau Online) then grab that
        //=============================================================================
        string candidateEmail = this.Name;
        if (RegExHelper.IsEmail(candidateEmail))
        {
            this.Email = candidateEmail;
        }
    }

    public override string ToString()
    {
        return "User: " + this.Name + "/" + this.Id + "/" + this.SiteRole;
    }

    string IHasSiteItemId.Id
    {
        get { return this.Id; }
    }
}
