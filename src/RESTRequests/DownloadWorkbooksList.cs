using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/// <summary>
/// Downloads the list of Workbooks from the server
/// </summary>
partial class DownloadWorkbooksList : TableauServerSignedInRequestBase
{
    public enum Sort
    {
        NoSort
//        , SortByUpdatedDateNewestFirst // [10/29/2019] This REST API (Get Workbooks for USER) does not yet support sorting

    }
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;
    private readonly Sort _sort;
    private readonly string _userIdForContentQuery;
    private readonly int _maxNumberItemsReturned;
    private readonly bool _filterToOwnedBy = false;

    /// <summary>
    /// Workbooks we've parsed from server results
    /// </summary>
    private List<SiteWorkbook> _workbooks;
    public ICollection<SiteWorkbook> Workbooks
    {
        get
        {
            var wb = _workbooks;
            if (wb == null) return null;
            return wb.AsReadOnly();
        }
    }

    /// <summary>
    /// Constructor: Call when we want to query the workbooks on behalf of the currently logged in user
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public DownloadWorkbooksList(TableauServerSignIn login, int maxNumberItems = int.MaxValue)
        : this(login, login.UserId, false, maxNumberItems)
    {
    }

    /// <summary>
    /// Constructor: Call when we want to query the Workbooks on behalf of an explicitly specified user
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="user"></param>
    public DownloadWorkbooksList(TableauServerSignIn login, string userId, bool filterToOwnedBy, int maxNumberItems = int.MaxValue) : base(login)
    {
        //Sanity test
        if(!RegExHelper.IsValidIdTableauContentId(userId))
        {
            throw new Exception("1030-139: Invalid user ID");
        }

        _onlineUrls = login.ServerUrls;
        _maxNumberItemsReturned = maxNumberItems;
        _sort = DownloadWorkbooksList.Sort.NoSort; //[2019-10-29] This is currently the only option
        _userIdForContentQuery = userId;
        _filterToOwnedBy = filterToOwnedBy;
    }


    /// <summary>
    /// Downloads a single request for 1 workbook
    /// </summary>
    /// <param name="workbookId"></param>
    /// <returns></returns>
    public SiteWorkbook ExecuteRequest_SingleWorkbook(string workbookId)
    {
        //Sanity check
        if (string.IsNullOrWhiteSpace(workbookId))
        {
            _onlineSession.StatusLog.AddError("Workbook ID required to query workbooks");
        }
        //Create a web request, in including the users logged-in auth information in the request headers
        var urlQuery = _onlineUrls.Url_WorkbookInfo(_onlineSession, workbookId);

        _onlineSession.StatusLog.AddStatus("Web request: " + urlQuery, -10);
        var xmlDoc = ResourceSafe_PerformWebRequest_GetXmlDocument(urlQuery, "get workbooks list");
        //var webRequest = CreateLoggedInWebRequest(urlQuery);
        //webRequest.Method = "GET";
        //var response = GetWebReponseLogErrors(webRequest, "get workbooks list");
        //var xmlDoc = GetWebResponseAsXml(response);

        //Get all the workbook nodes
        var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
        var workbooks = xmlDoc.SelectNodes("//iwsOnline:workbook", nsManager);

        //Get information for each of the data sources
        foreach (XmlNode itemXml in workbooks)
        {
            try
            {
                var wb = new SiteWorkbook(itemXml);
                //There is ONLY one workbook, so return it
                return wb;
            }
            catch
            {
                AppDiagnostics.Assert(false, "Workbook parse error");
                _onlineSession.StatusLog.AddError("Error parsing workbook: " + itemXml.InnerXml);
                return null;
            }
        } //end: foreach

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverName"></param>
    public void ExecuteRequest()
    {
        //Sanity check
        if(string.IsNullOrWhiteSpace(_userIdForContentQuery))
        {
            _onlineSession.StatusLog.AddError("User ID required to query workbooks");            
        }

        var onlineWorkbooks = new List<SiteWorkbook>();
        int numberPages = 1; //Start with 1 page (we will get an updated value from server)
        //Get subsequent pages
        for (int thisPage = 1; thisPage <= numberPages; thisPage++)
        {
            try
            {
                ExecuteRequest_ForPage(onlineWorkbooks, thisPage, out numberPages);
            }
            catch (Exception exPageRequest)
            {
                StatusLog.AddError("Workbooks error during page request: " + exPageRequest.Message);
            }

            //See if we already have anough items
            if(onlineWorkbooks.Count == _maxNumberItemsReturned)
            {
                goto exit_success;
            }
            else if (onlineWorkbooks.Count > _maxNumberItemsReturned)
            {
                //Remove any excess
                onlineWorkbooks.RemoveRange(_maxNumberItemsReturned, (onlineWorkbooks.Count - _maxNumberItemsReturned));
                goto exit_success;
            }
        }

exit_success:
        _workbooks = onlineWorkbooks;
    }


    /// <summary>
    /// Gives us a query string for the sort we want
    /// </summary>
    /// <returns></returns>
    private string SortDirectiveForContentQuery()
    {
        switch(_sort)
        {
            case Sort.NoSort:
                return "";
/*            case Sort.SortByUpdatedDateNewestFirst:
                return "sort=updatedAt:desc";
*/
            default:
                IwsDiagnostics.Assert(false, "Unknown workbook sort");
                throw new Exception("Unknown workbook sort");
        }
    }
    /// <summary>
    /// Get a page's worth of Workbook listings
    /// </summary>
    /// <param name="onlineWorkbooks"></param>
    /// <param name="pageToRequest">Page # we are requesting (1 based)</param>
    /// <param name="totalNumberPages">Total # of pages of data that Server can return us</param>
    private void ExecuteRequest_ForPage(List<SiteWorkbook> onlineWorkbooks, int pageToRequest, out int totalNumberPages)
    {
        int pageSize = _onlineUrls.PageSize;
        //Create a web request, in including the users logged-in auth information in the request headers
        var urlQuery = _onlineUrls.Url_WorkbooksListForUser(_onlineSession, _userIdForContentQuery, _filterToOwnedBy, pageSize, pageToRequest, SortDirectiveForContentQuery());

        _onlineSession.StatusLog.AddStatus("Web request: " + urlQuery, -10);
        var xmlDoc = ResourceSafe_PerformWebRequest_GetXmlDocument(urlQuery, "get workbooks list");
        //var webRequest = CreateLoggedInWebRequest(urlQuery);
        //webRequest.Method = "GET";
        //var response = GetWebReponseLogErrors(webRequest, "get workbooks list");
        //var xmlDoc = GetWebResponseAsXml(response);

        //Get all the workbook nodes
        var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
        var workbooks = xmlDoc.SelectNodes("//iwsOnline:workbook", nsManager);

        //Get information for each of the data sources
        foreach (XmlNode itemXml in workbooks)
        {
            try
            {
                var ds = new SiteWorkbook(itemXml);
                onlineWorkbooks.Add(ds);
            }
            catch
            {
                AppDiagnostics.Assert(false, "Workbook parse error");
                _onlineSession.StatusLog.AddError("Error parsing workbook: " + itemXml.InnerXml);
            }
        } //end: foreach

        //-------------------------------------------------------------------
        //Get the updated page-count
        //-------------------------------------------------------------------
        totalNumberPages = DownloadPaginationHelper.GetNumberOfPagesFromPagination(
            xmlDoc.SelectSingleNode("//iwsOnline:pagination", nsManager),
            pageSize);
    }

}
