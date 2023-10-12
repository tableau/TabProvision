using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;

/// <summary>
/// Download data from Azure about users and groups, so we can build a provision manifest file
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
        string emailCandidate;
        switch(_configSyncGroups.EmailMapping)
        {
            //Use the name of the principal in AzureAD
            case ProvisionConfigExternalDirectorySync.UserEmailMapping.UserPrincipalName:
                emailCandidate = graphUser.UserPrincipalName;
                IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1212: User principal name and 'mail' is NULL");
                return emailCandidate.Trim();

            case ProvisionConfigExternalDirectorySync.UserEmailMapping.Mail:
                emailCandidate = graphUser.Mail;
                IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1211: User 'mail' attribute is NULL");
                return emailCandidate.Trim();

            case ProvisionConfigExternalDirectorySync.UserEmailMapping.MailNickname:
                emailCandidate = graphUser.MailNickname;
                IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1210: User 'mailNickname' attribute is NULL for user: " + graphUser.UserPrincipalName);
                
                //If no email candidate was found, use the principal name
                if (string.IsNullOrWhiteSpace(emailCandidate))
                {
                    emailCandidate = graphUser.UserPrincipalName;
                    IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1209: User principal name is NULL");
                }

                return emailCandidate.Trim();

            case ProvisionConfigExternalDirectorySync.UserEmailMapping.OnPremisesSamAccountName:
                emailCandidate = graphUser.OnPremisesSamAccountName;
                IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1210: User 'onPremisesSamAccountName' attribute is NULL for user: " + graphUser.UserPrincipalName);
                
                //If no email candidate was found, use the principal name
                if (string.IsNullOrWhiteSpace(emailCandidate))
                {
                    emailCandidate = graphUser.UserPrincipalName;
                    IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1209: User principal name is NULL");
                }

                return emailCandidate.Trim();

            //If the user has another email address listed, use it
            case ProvisionConfigExternalDirectorySync.UserEmailMapping.PreferAzureProxyPrimaryEmail:
                emailCandidate = GetUserEmailFromGraphADUser_TryPrimaryProxyEmailLookup(graphUser);
                
                //If no email candidate was found, use the principal name
                if (string.IsNullOrWhiteSpace(emailCandidate))
                {
                    emailCandidate = graphUser.UserPrincipalName;
                    IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "1009-1209: User principal name is NULL");
                }
                return emailCandidate.Trim();


            default: //Unknown mode
                IwsDiagnostics.Assert(false, "1009-1208: Unknown user email mapping mode");
                throw new Exception("1009-1208: Unknown user email mapping mode");
        }
    }

    /// <summary>
    /// If the user has a proxy name specified as email, then use it
    /// </summary>
    /// <param name="graphUser"></param>
    /// <returns></returns>
    private string GetUserEmailFromGraphADUser_TryPrimaryProxyEmailLookup(User graphUser)
    {
        var proxyNamesSet = graphUser.ProxyAddresses;
        //If there are not proxy names specified, then there is nothing to do
        if(proxyNamesSet == null)
        {
            return null;
        }
        
        //Loop through all the proxy names and see if there is an email address
        foreach(var thisProxyName in proxyNamesSet)
        {
            //Sanity condition.....
            if(!string.IsNullOrEmpty(thisProxyName))
            {
                string thisProxyName_asEmail =
                    GetUserEmailFromGraphADUser_TryProxyEmailLookup_TryPrimaryEmailParse(thisProxyName);

                if (!string.IsNullOrEmpty(thisProxyName_asEmail))
                {
                    //SUCCESS:
                    return thisProxyName_asEmail; //It's an email, returnit
                }
            }
        }//end: foreach

        //NO Email addreses found
        return null;
    }

    /// <summary>
    /// Attempt to parse the primary email from the (optional) Azure proxy name
    /// </summary>
    /// <param name="thisProxyName"></param>
    /// <returns></returns>
    private string GetUserEmailFromGraphADUser_TryProxyEmailLookup_TryPrimaryEmailParse(string thisProxyName)
    {
        //See: https://docs.microsoft.com/en-us/troubleshoot/azure/active-directory/proxyaddresses-attribute-populate

        //Email protocol prefix
        const string prefix_smtp_primary = "SMTP:";
        //const string prefix_smtp_seconday = "smtp:"; //Currently we don't care about the secondary address
        //If it's blank it can't be an email
        if (string.IsNullOrWhiteSpace(thisProxyName))
        {
            return null;
        }

        //===============================================================
        //If it starts with SMTP: it's an email address
        //===============================================================
        if(thisProxyName.StartsWith(prefix_smtp_primary))
        {
            //Strip off the prefix
            thisProxyName.Substring(prefix_smtp_primary.Length);
        }

        return null;
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
