using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

partial class DownloadRecentAccessedContentList : TableauServerSignedInRequestBase
{
    /// <summary>
    /// Contains recent results
    /// </summary>
    public class RecentContentReport
    {
        public readonly ICollection<SiteWorkbook> Workbooks;
        public readonly ICollection<SiteView> Views;

        public RecentContentReport(ICollection<SiteWorkbook> workbooks, ICollection<SiteView> views)
        {
            this.Workbooks = workbooks;
            this.Views = views;
        }
    }

}
