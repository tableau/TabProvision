using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;

class SendCreateGroup : TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    private readonly string _groupName;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public SendCreateGroup(
        TableauServerSignIn login,
        string groupName)
        : base(login)
    {
        _onlineUrls = login.ServerUrls;
        _groupName = groupName;

    }

    /// <summary>
    /// Create a group on server
    /// </summary>
    /// <param name="serverName"></param>
    public SiteGroup ExecuteRequest()
    {
        try
        {
            var newGroup = CreateGroup(_groupName);
            this.StatusLog.AddStatus("Group created. " + newGroup.ToString());
            return newGroup;
        }
        catch (Exception ex)
        {
            this.StatusLog.AddError("Error attempting to create user '" + _groupName+ "', " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Create the group on the server
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    private SiteGroup CreateGroup(string groupName)
    {

        //ref: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#create_group
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
            xmlWriter.WriteStartElement("group");
                xmlWriter.WriteAttributeString("name", groupName);
            xmlWriter.WriteEndElement();//</user>
        xmlWriter.WriteEndElement(); //</tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Generate the MIME message

        //Create a web request 
        var urlCreateGroup = _onlineUrls.Url_CreateSiteGroup(_onlineSession);
        var webRequest = this.CreateLoggedInWebRequest(urlCreateGroup, "POST");
        TableauServerRequestBase.SendPostContents(webRequest, xmlText);

        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "create group");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);

            //Get all the workbook nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeGroup = xmlDoc.SelectSingleNode("//iwsOnline:group", nsManager);

            try
            {
                return new SiteGroup(xNodeGroup, null);
            }
            catch (Exception exParseXml)
            {
                StatusLog.AddError("Create user, error parsing XML response " + exParseXml.Message + "\r\n" + xNodeGroup.InnerXml);
                return null;
            }

        }
    }



}
