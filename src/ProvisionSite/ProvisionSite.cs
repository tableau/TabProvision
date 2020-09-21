using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provision a site
/// </summary>
internal partial class ProvisionSite
{
    readonly IShowLogs _showLogsHere;
    readonly TaskStatusLogs _statusLogs;
    readonly ProvisionConfigSiteAccess _config;
    readonly ProvisionUserInstructions _provisionInstructions;

    readonly CsvDataGenerator _csvProvisionResults = new CsvDataGenerator();
    /// <summary>
    /// CSV for generated report
    /// </summary>
    public CsvDataGenerator CSVResultsReport
    {
        get
        {
            return _csvProvisionResults;
        }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="config"></param>
    /// <param name="provisionInstructions"></param>
    /// <param name="showLogsHere"></param>
    /// <param name="statusLogs"></param>
    public ProvisionSite(ProvisionConfigSiteAccess config, ProvisionUserInstructions provisionInstructions, IShowLogs showLogsHere, TaskStatusLogs statusLogs)
    {
        _showLogsHere = showLogsHere;
        _config = config;
        _provisionInstructions = provisionInstructions;

        if (statusLogs == null)
        {
            statusLogs = new TaskStatusLogs();
        }
        _statusLogs = statusLogs;
    }

    /// <summary>
    /// Pushes status logs to show the user
    /// </summary>
    /// <param name="statusLogs"></param>
    private void ShowLogs()
    {
        //If we don't have a sender or a listener, then do nothing
        if (_statusLogs == null) return;
        if (_showLogsHere == null) return;

        _showLogsHere.NewLogResultsToShow(_statusLogs);
    }


    /// <summary>
    /// Queries a site for all its users, and for recent content.
    /// Genterates and sends email to all the users
    /// </summary>
    /// <param name="showLogsHere"></param>
    /// <param name="statusLogs"></param>
    public void Execute()
    {
        //==================================================================================
        //Get the data we need to run
        //==================================================================================
        //Generate the URLs we will need
        var siteUrlManager = TableauServerUrls.FromContentUrl(_config.SiteUrl, TaskMasterOptions.RestApiReponsePageSizeDefault);

        //=================================================================================
        //Sign in to the site
        //=================================================================================
        var siteSignIn = new TableauServerSignIn(
            siteUrlManager,
            _config.SiteClientId,
            _config.Secret,
            _statusLogs,
            _config.SiteSignInMode);

        var signInSuccess = siteSignIn.Execute();
        ShowLogs();

        //=================================================================================
        //Get the basic site info
        //=================================================================================
        var downloadSiteInfo = new DownloadSiteInfo(siteSignIn);
        downloadSiteInfo.ExecuteRequest();
        var siteInfo = downloadSiteInfo.Site;

        ShowLogs();
        //=================================================================================
        //Provision the users
        //=================================================================================
        var workingList_allKnownUsers = Execute_ProvisionUsers(siteSignIn);
        ShowLogs();

        //=================================================================================
        //Provision the groups
        //=================================================================================
        Execute_ProvisionGroups(siteSignIn, workingList_allKnownUsers);
    }




    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_WarningUpdatingUser(string userName, string userRole, string userAuth, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area", "user-name", "user-role", "user-auth", "notes" },
            new string[] { "warning", userName, userRole, userAuth, notes });
    }

    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_ErrorUpdatingUser(string userName, string userRole, string userAuth, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area", "user-name", "user-role", "user-auth", "notes" },
            new string[] { "error", userName, userRole, userAuth, notes });
    }

    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_ErrorUpdatingGroup(string groupName, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area", "group-name", "notes" },
            new string[] { "error", groupName  , notes });
    }

}
