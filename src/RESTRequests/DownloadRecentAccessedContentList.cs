using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Get's the list of recently accessed Workbooks and Views in the site.
/// 
/// </summary>
partial class DownloadRecentAccessedContentList : TableauServerSignedInRequestBase
{

    private readonly TableauServerUrls _onlineUrls;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public DownloadRecentAccessedContentList(TableauServerUrls onlineUrls, TableauServerSignIn login) 
        : base(login)
    {
        _onlineUrls = onlineUrls;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverName"></param>
    public RecentContentReport ExecuteRequest()
    {
        //Create a web request, in including the users logged-in auth information in the request headers
        var urlQuery = _onlineUrls.Url_RecentContentList(_onlineSession);

        _onlineSession.StatusLog.AddStatus("Web request: " + urlQuery, -10);
        XmlDocument xmlDoc = ResourceSafe_PerformWebRequest_GetXmlDocument(urlQuery, "get recent content list");
        //var webRequest = CreateLoggedInWebRequest(urlQuery);
        //webRequest.Method = "GET";
        //var response = GetWebReponseLogErrors(webRequest, "get recent content list");
        //var xmlDoc = GetWebResponseAsXml(response);

        //XPath requires us to have a namespace that maps to tableau's namespace
        var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");

        //Get the recent workbooks....
        var xmlWorkbooksList = xmlDoc.SelectNodes("//iwsOnline:recent/iwsOnline:workbook", nsManager);
        var recentWorkbooks = GenerateWorkbooksList(xmlWorkbooksList);

        //Get the recent views....
        var xmlViewsList = xmlDoc.SelectNodes("//iwsOnline:recent/iwsOnline:view", nsManager);
        var recentViews = GenerateViewsList(xmlViewsList);

        //Pack it up and return it
        return new RecentContentReport(recentWorkbooks, recentViews);
    }

    /// <summary>
    /// Build a list of Views 
    /// </summary>
    /// <param name="xmlViewsList"></param>
    /// <returns></returns>
    private List<SiteView> GenerateViewsList(XmlNodeList xmlViewsList)
    {
        var onlineViews = new List<SiteView>();
        //Get information for each of the data sources
        foreach (XmlNode itemXml in xmlViewsList)
        {
            try
            {
                var thisItem = new SiteView(itemXml);
                onlineViews.Add(thisItem);
            }
            catch
            {
                AppDiagnostics.Assert(false, "1024-1113: Workbook parse error");
                _onlineSession.StatusLog.AddError("1024-1113: Error parsing workbook: " + itemXml.InnerXml);
            }
        } //end: foreach

        return onlineViews;
    }


    /// <summary>
    /// Build a list of Workbooks 
    /// </summary>
    /// <param name="xmlWorkbooksList"></param>
    /// <returns></returns>
    private List<SiteWorkbook> GenerateWorkbooksList(XmlNodeList xmlWorkbooksList)
    {
        var onlineWorkbooks = new List<SiteWorkbook>();
        //Get information for each of the data sources
        foreach (XmlNode itemXml in xmlWorkbooksList)
        {
            try
            {
                var thisItem = new SiteWorkbook(itemXml);
                onlineWorkbooks.Add(thisItem);
            }
            catch
            {
                AppDiagnostics.Assert(false, "1024-1113: Workbook parse error");
                _onlineSession.StatusLog.AddError("1024-1113: Error parsing workbook: " + itemXml.InnerXml);
            }
        } //end: foreach

        return onlineWorkbooks;
    }

}
