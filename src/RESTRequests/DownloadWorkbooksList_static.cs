using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

partial class DownloadWorkbooksList : TableauServerSignedInRequestBase
{
    /*
    /// <summary>
    /// Queries for the set of workbooks that the specified user can access
    /// </summary>
    /// <param name="signIn"></param>
    /// <param name="queryForUserId"></param>
    /// <returns></returns>
    public static DownloadWorkbooksList CreateAndExecute(TableauServerSignIn signIn, string queryForUserId)
    {
        if(!RegExHelper.IsValidIdTableauContentId(queryForUserId))
        {
            throw new Exception("1030-910: User id syntax invalid: " + queryForUserId);
        }

        var downloader = new DownloadWorkbooksList(signIn, queryForUserId);
        downloader.ExecuteRequest();
        return downloader;
    }
    */
}
