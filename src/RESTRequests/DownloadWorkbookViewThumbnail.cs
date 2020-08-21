using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Downloads a workbook thumbnail image
/// </summary>
partial class DownloadWorkbookViewThumbnail : TableauServerSignedInRequestBase
{
    /// <summary>
    /// URL manager
    /// </summary>
    private readonly TableauServerUrls _onlineUrls;

    /// <summary>
    /// Workbooks we've parsed from server results
    /// </summary>
    private readonly SiteWorkbook _workbook;

    /// <summary>
    /// Local path where we are going to save downloaded workbooks to
    /// </summary>
    private readonly string _localSavePathRoot;

    /// <summary>
    /// ID of the view we want to get a thumbnail for
    /// </summary>
    private readonly string _viewId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="workbook"></param>
    /// <param name="localSavePath"></param>
    public DownloadWorkbookViewThumbnail(
        TableauServerUrls onlineUrls,
        TableauServerSignIn login,
        SiteWorkbook workbook,
        string viewId,
        string localSavePath)
        : base(login)
    {
        if(string.IsNullOrWhiteSpace(viewId))
        {
            var errorText = "Download workbook view thumbnail, viewId cannot be blank.  Workbook: " + workbook.Id;
            login.StatusLog.AddError(errorText);
            throw new Exception(errorText);
        }

        _onlineUrls = onlineUrls;
        _workbook = workbook;
        _localSavePathRoot = localSavePath;
        _viewId = viewId;
    }

    /// <summary>
    /// Run the command
    /// </summary>
    /// <returns></returns>
    public string ExecuteRequest()
    {
        var statusLog = _onlineSession.StatusLog;
        var workbook = _workbook;
        string viewId = _viewId;
        var saveDirectory = _localSavePathRoot;

        if ((workbook == null) || (string.IsNullOrWhiteSpace(workbook.Id)))
        {
            statusLog.AddError("1025-851: NULL workbook. Aborting download.");
            return null;
        }

        //Local path save the workbook
        string urlDownload = _onlineUrls.Url_WorkbookViewThumbnailDownload(_onlineSession, workbook, viewId);
        statusLog.AddStatus("Starting Workbook view thumbnail download " + workbook.Name + " " + workbook.ToString() + ", viewId: " + viewId);
        try
        {
            var fileDownloaded = this.DownloadFile(
                urlDownload,
                saveDirectory,
                ThumbnailFilenameWithoutExtension(viewId),
                DownloadPayloadTypeHelper.CreateForImageDownload());
            var fileDownloadedNoPath = System.IO.Path.GetFileName(fileDownloaded);

            statusLog.AddStatus("Finished Workbook view thumbnail download " + fileDownloadedNoPath);
            return fileDownloaded;

        }
        catch (Exception ex)
        {
            statusLog.AddError("1029-849: Error during Workbook view thumbnial download " + workbook.Name + "\r\n  " + urlDownload + "\r\n  " + ex.ToString());
            return null; //Thumbnail not downloaded
        }
    }


    /// <summary>
    /// Returns the base filename without an extension
    /// </summary>
    /// <param name="viewId"></param>
    /// <returns></returns>
    public static string ThumbnailFilenameWithoutExtension(string viewId)
    {
        return "view_" + viewId;
    }

}
