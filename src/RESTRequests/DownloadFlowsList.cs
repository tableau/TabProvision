using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/// <summary>
/// Downloads the list of Flows from the server
/// </summary>
partial class DownloadFlowsList : TableauServerSignedInRequestBase
{
    /*
    public enum Sort
    {
        NoSort
    }
    */
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;
    //private readonly Sort _sort;
    //private readonly string _userIdForContentQuery;
    private readonly int _maxNumberItemsReturned;
//    private readonly bool _filterToOwnedBy = false;

    /// <summary>
    /// Flows we've parsed from server results
    /// </summary>
    private List<SiteFlow> _flows;
    public ICollection<SiteFlow> Flows
    {
        get
        {
            var wb = _flows;
            if (wb == null) return null;
            return wb.AsReadOnly();
        }
    }

    /// <summary>
    /// Constructor: Call when we want to query the flows on behalf of the currently logged in user
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    public DownloadFlowsList(TableauServerSignIn login, int maxNumberItems = int.MaxValue)
        : base(login)
    {
        _onlineUrls = login.ServerUrls;
        _maxNumberItemsReturned = int.MaxValue;
    }

    /*
    /// <summary>
    /// Constructor: Call when we want to query the Flows on behalf of an explicitly specified user
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="user"></param>
    public DownloadFlowsList(TableauServerSignIn login, string userId, bool filterToOwnedBy, int maxNumberItems = int.MaxValue) : base(login)
    {
        //Sanity test
        if(!RegExHelper.IsValidIdTableauContentId(userId))
        {
            throw new Exception("1030-139: Invalid user ID");
        }

        _onlineUrls = login.ServerUrls;
        _maxNumberItemsReturned = maxNumberItems;
        _sort = DownloadFlowsList.Sort.NoSort; //[2019-10-29] This is currently the only option
        _userIdForContentQuery = userId;
        _filterToOwnedBy = filterToOwnedBy;
    }
    */
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverName"></param>
    public void ExecuteRequest()
    {
/*        //Sanity check
        if(string.IsNullOrWhiteSpace(_userIdForContentQuery))
        {
            _onlineSession.StatusLog.AddError("User ID required to query flows");            
        }
*/
        var onlineFlows = new List<SiteFlow>();
        int numberPages = 1; //Start with 1 page (we will get an updated value from server)
        //Get subsequent pages
        for (int thisPage = 1; thisPage <= numberPages; thisPage++)
        {
            try
            {
                ExecuteRequest_ForPage(onlineFlows, thisPage, out numberPages);
            }
            catch (Exception exPageRequest)
            {
                StatusLog.AddError("Flows error during page request: " + exPageRequest.Message);
            }

            //See if we already have anough items
            if(onlineFlows.Count == _maxNumberItemsReturned)
            {
                goto exit_success;
            }
            else if (onlineFlows.Count > _maxNumberItemsReturned)
            {
                //Remove any excess
                onlineFlows.RemoveRange(_maxNumberItemsReturned, (onlineFlows.Count - _maxNumberItemsReturned));
                goto exit_success;
            }
        }

exit_success:
        _flows = onlineFlows;
    }

    /*
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
            default:
                IwsDiagnostics.Assert(false, "Unknown flow sort");
                throw new Exception("Unknown flow sort");
        }
    }
    */
    /// <summary>
    /// Get a page's worth of Flow listings
    /// </summary>
    /// <param name="onlineFlows"></param>
    /// <param name="pageToRequest">Page # we are requesting (1 based)</param>
    /// <param name="totalNumberPages">Total # of pages of data that Server can return us</param>
    private void ExecuteRequest_ForPage(List<SiteFlow> onlineFlows, int pageToRequest, out int totalNumberPages)
    {
        int pageSize = _onlineUrls.PageSize;
        //Create a web request, in including the users logged-in auth information in the request headers
//        var urlQuery = _onlineUrls.Url_FlowsListForUser(_onlineSession, _userIdForContentQuery, _filterToOwnedBy, pageSize, pageToRequest, SortDirectiveForContentQuery());
        var urlQuery = _onlineUrls.Url_FlowsList(_onlineSession, pageSize, pageToRequest);

        _onlineSession.StatusLog.AddStatus("Web request: " + urlQuery, -10);
        var xmlDoc = ResourceSafe_PerformWebRequest_GetXmlDocument(urlQuery, "get flows list");

        //Get all the flow nodes
        var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
        var flows = xmlDoc.SelectNodes("//iwsOnline:flow", nsManager);

        //Get information for each of the data sources
        foreach (XmlNode itemXml in flows)
        {
            try
            {
                var ds = new SiteFlow(itemXml);
                onlineFlows.Add(ds);
            }
            catch
            {
                AppDiagnostics.Assert(false, "Flow parse error");
                _onlineSession.StatusLog.AddError("Error parsing flow: " + itemXml.InnerXml);
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
