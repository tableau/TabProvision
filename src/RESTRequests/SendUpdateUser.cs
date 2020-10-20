using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;

/// <summary>
/// Attempts to update a user's information on the server
/// </summary>
class SendUpdateUser: TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    private readonly string _newRole;
    private readonly string _userId;
    private readonly SiteUserAuth _newAuthentication;
    private readonly bool _updateAuthSetting = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="userId">GUID</param>
    /// <param name="newRole">GUID</param>
    public SendUpdateUser(
        TableauServerSignIn login,
        string userId,
        string newRole,
        bool updateAuthSetting,
        SiteUserAuth newAuthentication)
        : base(login)
    {
        _onlineUrls = login.ServerUrls;
        _userId = userId;
        _newRole = newRole;
        _newAuthentication = newAuthentication;
        _updateAuthSetting = updateAuthSetting;
    }

    /*
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="userId">GUID</param>
    /// <param name="newRole">GUID</param>
    public SendUpdateUser(
        TableauServerSignIn login,
        string userId,
        string newRole)
        : this(
              login, 
              userId, 
              newRole, 
              false, 
              SiteUserAuth.Unknown //This is a "don't care" value since we are not updating the role
              )
    {
    }
    */
    /// <summary>
    /// Change the user
    /// </summary>
    /// <param name="serverName"></param>
    public SiteUser ExecuteRequest()
    {
        try
        {
            var updatedUser = UpdateUser(_userId, _newRole, _updateAuthSetting, _newAuthentication);
            this.StatusLog.AddStatus("Site user updated:" + updatedUser.Name + "/" + updatedUser.Id +  ", role:" + updatedUser.SiteRole + ", auth:" + updatedUser.SiteAuthentication );
            return updatedUser;
        }
        catch (Exception exError)
        {
            this.StatusLog.AddError("Error attempting to update user'" + _userId + "' owner to '" + _newRole + "', " + exError.Message);
            return null;
        }
    }


    private SiteUser UpdateUser(string userId, string newRole, bool updateAuthentication, SiteUserAuth newAuthentication)
    {
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(userId), "missing user id");
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(newRole), "missing role");


        //ref: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#update_user
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
        xmlWriter.WriteStartElement("user");
            xmlWriter.WriteAttributeString("siteRole", newRole);

            //Only if we are updating the user's authentication method do we need to update this
            if(updateAuthentication)
            {
                string newAuthenticationText = SendCreateUser.SiteUserAuthToAttributeText(newAuthentication);
                xmlWriter.WriteAttributeString("authSetting", newAuthenticationText);
            }
        xmlWriter.WriteEndElement();//</user>
        xmlWriter.WriteEndElement(); // </tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Create a web request 
        var urlUpdateUser = _onlineUrls.Url_UpdateSiteUser(_onlineSession, userId);
        var webRequest = this.CreateLoggedInWebRequest(urlUpdateUser, "PUT");
        TableauServerRequestBase.SendPutContents(webRequest, xmlText);
        
        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "update user (change auth or role)");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);

            
            //Get all the user nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeUser = xmlDoc.SelectSingleNode("//iwsOnline:user", nsManager);

            try
            {
                if(updateAuthentication)
                {
                    return SiteUser.FromUserXMLWithoutUserId(xNodeUser, userId);
                }
                else
                {
                    //Use the passed in authentication
                    return SiteUser.FromUserXMLWithoutUserIdOrAuthRole(xNodeUser, userId, newAuthentication);
                }
            }
            catch (Exception parseXml)
            {
                StatusLog.AddError("Update user, error parsing XML response " + parseXml.Message + "\r\n" + xNodeUser.InnerXml);
                return null;
            }            
        }
    }


}
