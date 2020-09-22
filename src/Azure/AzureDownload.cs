using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;
//using Microsoft.Azure.ActiveDirectory.GraphClient;
//using Microsoft.Azure.ActiveDirectory.GraphClient.Internal;

/// <summary>
/// Provision a site
/// </summary>
internal partial class AzureDownload
{
    readonly IShowLogs _showLogsHere;
    readonly TaskStatusLogs _statusLogs;
    readonly AzureAdConfig _configAzure;
    readonly ProvisionConfigExternalDirectorySync _configSyncGroups;
    public SimpleLatch IsExecuteComplete = new SimpleLatch();

        /// <summary>
    /// The calculated sets of users
    /// </summary>
    public ProvisionFromDirectoryManager ProvisioningManifestResults = new ProvisionFromDirectoryManager();

    readonly CsvDataGenerator _csvProvisionResults = null;
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
    /// <param name="configSyncGroups"></param>
    /// <param name="showLogsHere"></param>
    /// <param name="statusLogs"></param>
    /// <param name="csvDataGenerator"></param>
    public AzureDownload(
        AzureAdConfig config, 
        ProvisionConfigExternalDirectorySync configSyncGroups, 
        IShowLogs showLogsHere, 
        TaskStatusLogs statusLogs, 
        CsvDataGenerator csvDataGenerator)
    {
        _showLogsHere = showLogsHere;
        _configAzure = config;
        _configSyncGroups = configSyncGroups;

        if (statusLogs == null)
        {
            statusLogs = new TaskStatusLogs();
        }
        _statusLogs = statusLogs;

        //Either use one passed in, or create one
        if (csvDataGenerator == null)
        {
            csvDataGenerator = new CsvDataGenerator();
        }
        _csvProvisionResults = csvDataGenerator;

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
        //Create placeholder groups
        EnsureTrackingGroupsExist_Roles();
        EnsureTrackingGroupsExist_Groups();

        //Run the work and wait for it to complete
        var t = Task.Run(async () => await Execute_Async());
        t.Wait();
    }

    /// <summary>
    /// Inner wrapper for async operations
    /// </summary>
    /// <returns></returns>
    private async Task Execute_Async()
    {
        var azureGraphSession = AzureGetGraphSession();

        //===================================================================================
        //Get Groups that map to Tableau Site User Roles from Azure
        //===================================================================================
        _statusLogs.AddStatus("Azure: Getting user roles groups");
        await GenerateUsersRolesList_FromAzureGroupsSyncList(azureGraphSession, _configSyncGroups.GroupsToRolesSyncList);

        //===================================================================================
        //Get Groups that map to Tableau Site Groups from Azure
        //===================================================================================
        _statusLogs.AddStatus("Azure: Getting user roles groups");
        await GenerateGroupsMembersList_FromAzureGroupsSyncList(azureGraphSession, _configSyncGroups.GroupsToGroupsSyncList);

        //===================================================================================
        //Now perform any replace/override operations we need to based on explicit users
        //===================================================================================
        _statusLogs.AddStatus("Replacing any explicit user/role overrides");
        foreach (var thisOverrideUser in _configSyncGroups.UserRolesOverrideList)
        {
            SetManagerForRoles.AddAndForceReplaceUser(thisOverrideUser);
        }

        //Mark the work status as complete
        IsExecuteComplete.Trigger();
    }


    /// <summary>
    /// Get the user's email from the Graph API's User object
    /// </summary>
    /// <param name="graphUser"></param>
    /// <returns></returns>
    private string GetUserEmailFromGraphADUser(Microsoft.Graph.User graphUser)
    {
        string emailCandidate = graphUser.UserPrincipalName;
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "814-705: User principal name is NULL");
        return emailCandidate.Trim();
    }

    /// <summary>
    /// Generate a Microsoft Graph session
    /// </summary>
    /// <returns></returns>
    private GraphServiceClient AzureGetGraphSession()
    {
        _statusLogs.AddStatus("Azure sign in");
        string azureSessionToken = AzureSignInGetAccessToken();

        //Note: This generates a CS1998 compiler warning, incidating that the method is marked 'async' but does 
        //not actually do any asynchonous work, and so does not need to be marked 'async'.  This is OK.
        //The surrounding method requires a function signature that looks like this (i.e. allows async work),
        //so we marks the function as 'async' even though it does not need to be.
#pragma warning disable 1998
        GraphServiceClient graphClient = new GraphServiceClient(
            "https://graph.microsoft.com/v1.0/" + _configAzure.AzureAdTenantId,
             new DelegateAuthenticationProvider(async (requestMessage) => {
                 requestMessage.Headers.Authorization = new
                          System.Net.Http.Headers.AuthenticationHeaderValue("bearer", azureSessionToken);
             }));
#pragma warning restore 1998

        return graphClient;
    }

    /// <summary>
    /// Sign in to Azure and get the access token for this session
    /// </summary>
    private string AzureSignInGetAccessToken()
    {
        string urlAuth = "https://login.microsoftonline.com/" + _configAzure.AzureAdTenantId;
        AuthenticationContext authenticationContext = new AuthenticationContext(urlAuth, true);
        ClientCredential clientCred = new ClientCredential(_configAzure.AzureAdClientId, _configAzure.AzureAdClientSecret);

        const string AzureGraphAPI = "https://graph.microsoft.com";
        var authResultAsync = authenticationContext.AcquireTokenAsync(AzureGraphAPI, clientCred);
        authResultAsync.Wait();

        return authResultAsync.Result.AccessToken;
    }
}
