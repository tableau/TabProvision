using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Downloads a workbook thumbnail image
/// </summary>
partial class DownloadWorkbookThumbnail : TableauServerSignedInRequestBase
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
    /// Constructor
    /// </summary>
    /// <param name="onlineUrls"></param>
    /// <param name="login"></param>
    /// <param name="workbook"></param>
    /// <param name="localSavePath"></param>
    public DownloadWorkbookThumbnail(
        TableauServerUrls onlineUrls,
        TableauServerSignIn login,
        SiteWorkbook workbook,
        string localSavePath)
        : base(login)
    {
        _onlineUrls = onlineUrls;
        _workbook = workbook;
        _localSavePathRoot = localSavePath;

    }

    /// <summary>
    /// Run the command
    /// </summary>
    /// <returns></returns>
    public string ExecuteRequest()
    {
        var statusLog = _onlineSession.StatusLog;
        var workbook = _workbook;
        var saveDirectory = _localSavePathRoot;

        if ((workbook == null) || (string.IsNullOrWhiteSpace(workbook.Id)))
        {
            statusLog.AddError("1025-851: NULL workbook. Aborting download.");
            return null;
        }

        //Local path save the workbook
        string urlDownload = _onlineUrls.Url_WorkbookThumbnailDownload(_onlineSession, workbook);
        statusLog.AddStatus("Starting Workbook thumbnail download " + workbook.Name + " " + workbook.ToString());
        try
        {
            var fileDownloaded = this.DownloadFile(
                urlDownload,
                saveDirectory,
                workbook.Id,
                DownloadPayloadTypeHelper.CreateForImageDownload());
            var fileDownloadedNoPath = System.IO.Path.GetFileName(fileDownloaded);

            statusLog.AddStatus("Finished Workbook thumbnail download " + fileDownloadedNoPath);
            return fileDownloaded;

        }
        catch (Exception ex)
        {
            statusLog.AddError("1025-1035: Error during Workbook thumbnial download " + workbook.Name + "\r\n  " + urlDownload + "\r\n  " + ex.ToString());
            return null; //Thumbnail not downloaded
        }
    }

}
