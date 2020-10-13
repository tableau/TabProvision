using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;

/// <summary>
/// Download data from Tableau (Online/Server) about users and groups, so we can build a provision manifest file
/// </summary>
internal partial class TableauProvisionDownload
{
    readonly IShowLogs _showLogsHere;
    readonly TaskStatusLogs _statusLogs;
    readonly ProvisionConfigSiteAccess _configTableauSecrets;
    readonly bool _ignoreAllUsersGroupInExport = true;

    /// <summary>
    /// The calculated sets of users
    /// </summary>
    public ProvisionFromDirectoryManager ProvisioningManifestResults = new ProvisionFromDirectoryManager();

    /// <summary>
    /// The internal manager we use to track all the user roles for provisioning
    /// </summary>
    private ProvisionFromDirectoryRolesMembershipManager SetManagerForRoles
    {
        get
        {
            return this.ProvisioningManifestResults.RolesManager;
        }
    }

    /// <summary>
    /// The internal manager we use to track all the user roles for provisioning
    /// </summary>
    private ProvisionFromDirectoryGroupsMembershipManager SetManagerForGroups
    {
        get
        {
            return this.ProvisioningManifestResults.GroupsMembershipManager;
        }
    }

    //readonly CsvDataGenerator _csvProvisionResults = null;
    /*/// <summary>
    /// CSV for generated report
    /// </summary>
    public CsvDataGenerator CSVResultsReport
    {
        get
        {
            return _csvProvisionResults;
        }
    }
    */

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="config"></param>
    /// <param name="configSyncGroups"></param>
    /// <param name="showLogsHere"></param>
    /// <param name="statusLogs"></param>
    /// <param name="ignoreAllUsersGroup">(True recommended) Do not export the 'all users' group</param>
    public TableauProvisionDownload(
        ProvisionConfigSiteAccess config, 
        IShowLogs showLogsHere, 
        TaskStatusLogs statusLogs, 
        bool ignoreAllUsersGroup = true)
    {
        _ignoreAllUsersGroupInExport = ignoreAllUsersGroup;
        _showLogsHere = showLogsHere;
        _configTableauSecrets = config;

        if (statusLogs == null)
        {
            statusLogs = new TaskStatusLogs();
        }
        _statusLogs = statusLogs;

        //Either use one passed in, or create one
        /*if (csvDataGenerator == null)
        {
            csvDataGenerator = new CsvDataGenerator();
        }
        */
        //_csvProvisionResults = csvDataGenerator;

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
        var siteUrlManager = TableauServerUrls.FromContentUrl(_configTableauSecrets.SiteUrl, TaskMasterOptions.RestApiReponsePageSizeDefault);

        //=================================================================================
        //Sign in to the site
        //=================================================================================
        var siteSignIn = new TableauServerSignIn(
            siteUrlManager,
            _configTableauSecrets.SiteClientId,
            _configTableauSecrets.Secret,
            _statusLogs,
            _configTableauSecrets.SiteSignInMode);

        var signInSuccess = siteSignIn.Execute();
        ShowLogs();

        //=================================================================================
        //Get the basic site info
        //=================================================================================
        var downloadSiteInfo = new DownloadSiteInfo(siteSignIn);
        downloadSiteInfo.ExecuteRequest();
        var siteInfo = downloadSiteInfo.Site;

        //===================================================================================
        //Get Groups that map to Tableau Site User Roles from Tableau
        //===================================================================================
        _statusLogs.AddStatus("Tableau: Getting user roles groups");
        GenerateUsersRolesList_FromTableauSite(siteSignIn);

        //===================================================================================
        //Get Groups that map to Tableau Site Groups from Tableau
        //===================================================================================
        _statusLogs.AddStatus("Tableau: Getting user roles groups");
        GenerateGroupsMembersList_FromTableauSite(siteSignIn);

    }

    /// <summary>
    /// Download the ist of users/roles
    /// </summary>
    /// <param name="siteSignIn"></param>
    private void GenerateUsersRolesList_FromTableauSite(TableauServerSignIn siteSignIn)
    {
        var downloadUsers = new DownloadUsersList(siteSignIn);
        bool downloadSuccess = downloadUsers.ExecuteRequest();
        if(!downloadSuccess)
        {
            throw new Exception("1012-358: Fatal error attempting to download users");
        }

        var userRolesManager = this.SetManagerForRoles;
        foreach(var thisUser in downloadUsers.Users)
        {
            var thisProvisioningUser = new ProvisioningUser(
                thisUser.Name,
                thisUser.SiteRole,
                thisUser.SiteAuthentication,
                "Tableau Online site list",
                false);

            userRolesManager.AddUser(thisProvisioningUser);
        }

    }


    const string SpecialGroup_AllUsers = "All Users";
    /// <summary>
    /// Download information about all the users and all the groups
    /// </summary>
    /// <param name="siteSignIn"></param>
    private void GenerateGroupsMembersList_FromTableauSite(TableauServerSignIn siteSignIn)
    {
        var downloadGroups = new DownloadGroupsList(siteSignIn);

        var downloadSuccess = downloadGroups.ExecuteRequest(true);
        if (!downloadSuccess)
        {
            throw new Exception("1012-512: Fatal error attempting to download groups");
        }

        //Loop through all the groups
        foreach (var thisGroup in downloadGroups.Groups)
        {
            if(GenerateGroupsMembersList_IsGroupToExport(thisGroup.Name))
            {
                GenerateGroupsMembersList_FromTableauSite_ProcessSingleGroup(thisGroup);
            }
        }
    }

    /// <summary>
    /// If TRUE, this is a group who's members we want to export
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    private bool GenerateGroupsMembersList_IsGroupToExport(string groupName)
    {
        //Presently the only group we may not want to export is the "All Users" group
        if(!_ignoreAllUsersGroupInExport)
        {
            return true;
        }

        //TRUE as long as the group's name is not "All Users"
        return (string.Compare(groupName, SpecialGroup_AllUsers, true) != 0);
    }

    /// <summary>
    /// Process a single group (exporting its members)
    /// </summary>
    /// <param name="thisGroup"></param>
    private void GenerateGroupsMembersList_FromTableauSite_ProcessSingleGroup(SiteGroup thisGroup)
    {
        //Get or create the group manager that will record the users in the group
        var thisGroupManager = this.SetManagerForGroups.GetManagerForGroup_CreateIfNeeded(
            thisGroup.Name,
            AttemptParseGrantLicenseText(thisGroup.GrantLicenseMode),
            thisGroup.SiteMinimumRoleOrNull
            );

        //Add each user to the group
        foreach (var thisUserInGroup in thisGroup.Users)
        {
            thisGroupManager.AddUser(thisUserInGroup.Name);
        }
    }

    /// <summary>
    /// Parse the Grant license text returned
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    ProvisioningGroup.GrantLicenseMode AttemptParseGrantLicenseText(string text)
    {
        //var parseGrantLicenseMode
        ProvisioningGroup.GrantLicenseMode grantLicenseMode = ProvisioningGroup.GrantLicenseMode.Ignore;
        try
        {
            return ProvisioningGroup.ParseGrantLicenseModeFromTableauServer(text);
        }
        catch(Exception ex)
        {
            //Note the error -- but it is not fatal for our purposes
            IwsDiagnostics.Assert(false, "1012-528: Unknown grant license mode parsing error, " + ex.Message);
            return grantLicenseMode;
        }

    }

}

