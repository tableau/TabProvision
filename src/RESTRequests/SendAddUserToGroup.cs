using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;

class SendAddUserToGroup : TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    private readonly string _userId;
    private readonly string _groupId = "";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public SendAddUserToGroup(
        TableauServerSignIn login,
        string userId,
        string groupId)
        : base(login)
    {
        _onlineUrls = login.ServerUrls;
        _userId = userId;
        _groupId = groupId;
    }

    /// <summary>
    /// Create a user on server
    /// </summary>
    /// <param name="serverName"></param>
    public bool ExecuteRequest()
    {
        try
        {
            var success = AddUserToGroup(_userId, _groupId);
            return success;
        }
        catch (Exception ex)
        {
            this.StatusLog.AddError("Error attempting to add user to group'" + _userId+ "', " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Create the user on the server
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <param name="userAuthentication"></param>
    /// <returns>TRUE: Suceess</returns>
    private bool AddUserToGroup(string userId, string groupId)
    {
        //ref: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#add_user_to_group
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
            xmlWriter.WriteStartElement("user");
                xmlWriter.WriteAttributeString("id", userId);
            xmlWriter.WriteEndElement();//</user>
        xmlWriter.WriteEndElement(); //</tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Generate the MIME message

        //Create a web request 
        var urlAddUserToGroup = _onlineUrls.Url_AddUserToGroup(_onlineSession, groupId);
        var webRequest = this.CreateLoggedInWebRequest(urlAddUserToGroup, "POST");
        TableauServerRequestBase.SendPostContents(webRequest, xmlText);

        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "add user to group");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);

            //Get all the workbook nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeUser = xmlDoc.SelectSingleNode("//iwsOnline:user", nsManager);

            if(xNodeUser == null)
            {
                StatusLog.AddError("Add user to group failed. Repsonse was not a USER node");
                return false; //Failure
            }

            return true; //Success
        }
    }



}
