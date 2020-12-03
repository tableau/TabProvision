using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;

/// <summary>
/// Attempts to update the owner of a flow on server
/// </summary>
class SendUpdateFlowOwner: TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    private readonly string _newOwnerId;
    private readonly string _flowId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="login"></param>
    /// <param name="flowId">GUID</param>
    /// <param name="newOwnerId">GUID</param>
    public SendUpdateFlowOwner(
        TableauServerSignIn login,
        string flowId,
        string newOwnerId)
        : base(login)
    {
        _onlineUrls = login.ServerUrls;
        _flowId = flowId;
        _newOwnerId = newOwnerId;
    }

    /// <summary>
    /// Change the owner of a flow on server
    /// </summary>
    /// <param name="serverName"></param>
    public SiteFlow ExecuteRequest()
    {
        try
        {
            var ds = ChangeContentOwner(_flowId, _newOwnerId);
            this.StatusLog.AddStatus("Flow ownership changed. ds:" + ds.Name + "/" + ds.Id +  ", new owner:" + ds.OwnerId);
            return ds;
        }
        catch (Exception exError)
        {
            this.StatusLog.AddError("Error attempting to change the flow '" + _flowId + "' owner to '" + _newOwnerId + "', " + exError.Message);
            return null;
        }
    }


    private SiteFlow ChangeContentOwner(string flowId, string newOwnerId)
    {
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(flowId), "missing flow id");
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(newOwnerId), "missing owner id");

        //ref: https://onlinehelp.tableau.com/current/api/rest_api/en-us/help.htm#REST/rest_api_ref.htm#Update_Flow%3FTocPath%3DAPI%2520Reference%7C_____76
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
        xmlWriter.WriteStartElement("flow");
            xmlWriter.WriteStartElement("owner");
               xmlWriter.WriteAttributeString("id", newOwnerId);  
            xmlWriter.WriteEndElement();//</owner>
        xmlWriter.WriteEndElement();//</flow>
        xmlWriter.WriteEndElement(); // </tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Create a web request 
        var urlUpdateFlow = _onlineUrls.Url_UpdateFlow(_onlineSession, flowId);
        var webRequest = this.CreateLoggedInWebRequest(urlUpdateFlow, "PUT");
        TableauServerRequestBase.SendPutContents(webRequest, xmlText);
        
        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "update flow (change owner)");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);

            
            //Get all the flow nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeDs = xmlDoc.SelectSingleNode("//iwsOnline:flow", nsManager);

            try
            {
                return new SiteFlow(xNodeDs);
            }
            catch (Exception parseXml)
            {
                StatusLog.AddError("Change flow owner, error parsing XML response " + parseXml.Message + "\r\n" + xNodeDs.InnerXml);
                return null;
            }
            
        }
    }


}
