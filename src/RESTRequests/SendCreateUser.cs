using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;

class SendCreateUser : TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    private readonly string _userName;
    private readonly string _userRole = "";
    private readonly SiteUserAuth _userAuthentication;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public SendCreateUser(
        TableauServerUrls onlineUrls, 
        TableauServerSignIn login,
        string userName,
        string userRole, //Creator, Explorer, ExplorerCanPublish, SiteAdministratorExplorer, SiteAdministratorCreator, Unlicensed, or Viewer
        SiteUserAuth userAuthentication)
        : base(login)
    {
        _onlineUrls = onlineUrls;
        _userName = userName;
        _userRole = userRole;
        _userAuthentication = userAuthentication; 

    }

    /// <summary>
    /// Create a user on server
    /// </summary>
    /// <param name="serverName"></param>
    public SiteUser ExecuteRequest()
    {
        try
        {
            var newUser = CreateUser(_userName, _userRole, _userAuthentication);
            this.StatusLog.AddStatus("User created. " + newUser.ToString());
            return newUser;
        }
        catch (Exception ex)
        {
            this.StatusLog.AddError("Error attempting to create user '" + _userName+ "', " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// XML Attribute text from the ennumeration value
    /// </summary>
    /// <param name="userAuthentication"></param>
    /// <returns></returns>
    public static string SiteUserAuthToAttributeText(SiteUserAuth userAuthentication)
    {
        switch (userAuthentication)
        {
            case SiteUserAuth.Default:
                return "ServerDefault";
            case SiteUserAuth.SAML:
                return "SAML";
            case SiteUserAuth.OpenID:
                return "OpenID";
            default:
                IwsDiagnostics.Assert(false, "810-1036: Unknown auth type for user ");
                throw new Exception("810-1036: Unknown auth type for user ");
        }
    }

    /// <summary>
    /// Create the user on the server
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuthentication"></param>
    /// <returns></returns>
    private SiteUser CreateUser(string userName, string userRole, SiteUserAuth userAuthentication)
    {
        string authSettingText = SiteUserAuthToAttributeText(userAuthentication);

        //ref: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#add_user_to_site
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
        xmlWriter.WriteStartElement("user");
        xmlWriter.WriteAttributeString("name", userName);
        xmlWriter.WriteAttributeString("siteRole", userRole);
        xmlWriter.WriteAttributeString("authSetting", authSettingText);
        xmlWriter.WriteEndElement();//</user>
        xmlWriter.WriteEndElement(); //</tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Generate the MIME message

        //Create a web request 
        var urlCreateUser = _onlineUrls.Url_CreateSiteUser(_onlineSession);
        var webRequest = this.CreateLoggedInWebRequest(urlCreateUser, "POST");
        TableauServerRequestBase.SendPostContents(webRequest, xmlText);

        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "create user");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);

            //Get all the workbook nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeProject = xmlDoc.SelectSingleNode("//iwsOnline:user", nsManager);

            try
            {
                return new SiteUser(xNodeProject);
            }
            catch (Exception parseXml)
            {
                StatusLog.AddError("Create user, error parsing XML response " + parseXml.Message + "\r\n" + xNodeProject.InnerXml);
                return null;
            }

        }
    }



}
