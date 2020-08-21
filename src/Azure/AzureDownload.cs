using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.Azure.ActiveDirectory.GraphClient.Internal;

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
    /// Contructor
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
    /// Makes a starting group for each Role Group we are provisioning for (so even empty groups get represented, so we know to keep them empty)
    /// </summary>
    private void EnsureTrackingGroupsExist_Roles()
    {
        foreach(var item in _configSyncGroups.GroupsToRolesSyncList)
        {
            this.SetManagerForRoles.EnsureRoleManagerExistsForRole(item.TableauRole);
        }
    }

    /// <summary>
    /// Makes a starting group for each Group we are provisioning for (so even empty groups get represented, so we know to keep them empty)
    /// </summary>
    private void EnsureTrackingGroupsExist_Groups()
    {
        foreach (var item in _configSyncGroups.GroupsToGroupsSyncList)
        {
            this.SetManagerForGroups.EnsureRoleManagerExistsForRole(item.TargetGroupName);
        }
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
        await GenerateUsersRolesListFromAzureGroups(azureGraphSession, _configSyncGroups.GroupsToRolesSyncList);

        //===================================================================================
        //Get Groups that map to Tableau Site Groups from Azure
        //===================================================================================
        _statusLogs.AddStatus("Azure: Getting user roles groups");
        await GeneratGroupsMembersListFromAzureGroups(azureGraphSession, _configSyncGroups.GroupsToGroupsSyncList);

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
    /// Query for each of the specified groups and pull down their members list from Azure
    /// </summary>
    /// <param name="azureSessionToken"></param>
    /// <param name="groupsToRolesSyncList"></param>
    private async Task GeneratGroupsMembersListFromAzureGroups(GraphServiceClient azureGraph, IEnumerable<ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup> groupsToGroupsSyncList)
    {
        foreach (var groupToRetrieve in groupsToGroupsSyncList)
        {
            _statusLogs.AddStatus("Azure getting group: " + groupToRetrieve.SourceGroupName);
            var thisGroupAsSet = await azureGraph.Groups.Request().Select(x => new { x.Id, x.DisplayName }).Filter("displayName eq '" + groupToRetrieve.SourceGroupName + "'").GetAsync();
            //If the group does not exist in Azure, not the error condition
            if(thisGroupAsSet.Count < 1)
            {
                _statusLogs.AddError("Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
                throw new Exception("814-722: Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
            }
            var thiGroupId = thisGroupAsSet.CurrentPage[0].Id;

            //https://docs.microsoft.com/en-us/graph/api/group-list-members?view=graph-rest-1.0&tabs=http
            //UNDONE: Filter down to just USERS and SUB-GROUPS

            //elect(x => new {x.UserPricipalName, x.DisplayName, x.Mail  }
            var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().GetAsync();

            //Get all the users in the group and sub-groups
            AzureRecurseGroupsGenerateGroupMembersList(azureGraph, thisGroupsMembers, groupToRetrieve);
        }
    }

    /// <summary>
    /// Itterate down a groups membership, looing in any sub-groups, and record all the members
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupsMembers"></param>
    /// <param name="baseGroupToRetrieve"></param>
    async void AzureRecurseGroupsGenerateGroupMembersList(GraphServiceClient azureGraph, IGroupMembersCollectionWithReferencesPage thisGroupsMembers, ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup baseGroupToRetrieve)
    {
        var thispage_members = thisGroupsMembers;
        do
        {
            if (thispage_members.Count > 0)
            {

                foreach (var thisMember in thispage_members)
                {
                    var asUser = thisMember as Microsoft.Graph.User;
                    var asSubGroup = thisMember as Microsoft.Graph.Group;
                    if (asUser != null)
                    {
                        AddUserToGroupProvisioningTrackingManager(
                            baseGroupToRetrieve.TargetGroupName,
                            asUser,
                            baseGroupToRetrieve);
                        //Add them to the list of users
                    }
                    else if (asSubGroup != null)
                    {
                        //-----------------------------------------------------------------------------------
                        //Recurse down the subgroup and get its members
                        //-----------------------------------------------------------------------------------
                        var subGroupsMembers = await azureGraph.Groups[asSubGroup.Id].Members.Request().GetAsync();
                        AzureRecurseGroupsGenerateGroupMembersList(azureGraph, subGroupsMembers, baseGroupToRetrieve);
                    }
                }
            }

            //Go to the next page
            if (thispage_members.NextPageRequest != null)
            {
                thispage_members = await thispage_members.NextPageRequest.GetAsync();
            }
            else
            {
                thispage_members = null;
            }

        } while (thispage_members != null);
    }

    /// <summary>
    /// Threadsafety lock for this function
    /// </summary>
    private object _lock_AddUserToGroupProvisioningTrackingManager = new object();
    /// <summary>
    /// Add a user to specified Group 
    /// </summary>
    /// <param name="targetGroupName"></param>
    /// <param name="asUser"></param>
    /// <param name="sourceGroupName"></param>
    private void AddUserToGroupProvisioningTrackingManager(string targetGroupName, Microsoft.Graph.User graphUser, ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup targetGroup)
    {
        //Because the request code can run async, and the collection management used is not designed to be thread-safe
        //we are going to serialize adding users to the collection.  
        lock (_lock_AddUserToGroupProvisioningTrackingManager)
        {
            AddUserToGroupProvisioningTrackingManager_Inner(targetGroupName, graphUser, targetGroup);
        }
    }
    

    /// <summary>
    /// Add the user to the tracking object
    /// </summary>
    /// <param name="targetGroupName"></param>
    /// <param name="graphUser"></param>
    /// <param name="sourceGroupName"></param>
    private void AddUserToGroupProvisioningTrackingManager_Inner(string targetGroupName, Microsoft.Graph.User graphUser, ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup targetGroup)
    {
        string emailCandidate = GetUserEmailFromGraphADUser(graphUser);
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "843-706: User principal name is NULL");

        //Add the user to our tracking set
        SetManagerForGroups.AddUserToGroup(targetGroup, emailCandidate);
    }


    /// <summary>
    /// Query for each of the specified groups and pull down their members list from Azure
    /// </summary>
    /// <param name="azureSessionToken"></param>
    /// <param name="groupsToRolesSyncList"></param>
    private async Task GenerateUsersRolesListFromAzureGroups(GraphServiceClient azureGraph, IEnumerable<ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole> groupsToRolesSyncList)
    {
        foreach(var groupToRetrieve in groupsToRolesSyncList)
        {
            _statusLogs.AddStatus("Azure getting role sync group: " + groupToRetrieve.SourceGroupName);

            var thisGroupAsSet = await azureGraph.Groups.Request().Select(x=> new {x.Id, x.DisplayName }).Filter("displayName eq '" + groupToRetrieve.SourceGroupName + "'").GetAsync();
            //If the group does not exist in Azure, not the error condition
            if (thisGroupAsSet.Count < 1)
            {
                _statusLogs.AddError("Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
                throw new Exception("814-723: Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
            }

            var thiGroupId = thisGroupAsSet.CurrentPage[0].Id;

            //https://docs.microsoft.com/en-us/graph/api/group-list-members?view=graph-rest-1.0&tabs=http
            //UNDONE: Filter down to just USERS and SUB-GROUPS

            //elect(x => new {x.UserPricipalName, x.DisplayName, x.Mail  }
            var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().GetAsync();

            //Get all the users in the group and sub-groups
            AzureRecurseGroupsGenerateRolesList(azureGraph, thisGroupsMembers, groupToRetrieve);
        }
    }

    /// <summary>
    /// Itterate down a groups membership, looing in any sub-groups, and record all the members
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupsMembers"></param>
    /// <param name="baseGroupToRetrieve"></param>
    async void AzureRecurseGroupsGenerateRolesList(GraphServiceClient azureGraph, IGroupMembersCollectionWithReferencesPage thisGroupsMembers, ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole baseGroupToRetrieve)
    {
        var thispage_members = thisGroupsMembers;
        do
        {
            if (thispage_members.Count > 0)
            {

                foreach (var thisMember in thispage_members)
                {
                    var asUser = thisMember as Microsoft.Graph.User;
                    var asSubGroup = thisMember as Microsoft.Graph.Group;
                    if (asUser != null)
                    {
                        AddUserToRoleProvisioningTrackingManager(
                            baseGroupToRetrieve.TableauRole,
                            baseGroupToRetrieve.AuthenticationModel,
                            asUser,
                            baseGroupToRetrieve.SourceGroupName);
                        //Add them to the list of users
                    }
                    else if(asSubGroup != null)
                    {
                        //-----------------------------------------------------------------------------------
                        //Recurse down the subgroup and get its members
                        //-----------------------------------------------------------------------------------
                        var subGroupsMembers = await azureGraph.Groups[asSubGroup.Id].Members.Request().GetAsync();
                        AzureRecurseGroupsGenerateRolesList(azureGraph, subGroupsMembers, baseGroupToRetrieve);
                    }

                }

            }

            //Go to the next page
            if (thispage_members.NextPageRequest != null)
            {
                thispage_members = await thispage_members.NextPageRequest.GetAsync();
            }
            else
            {
                thispage_members = null;
            }

        } while (thispage_members != null);
    }


    /// <summary>
    /// Threadsafety lock for this function
    /// </summary>
    private object _lock_AddUserToRoleProvisioningTrackingManager = new object();

    /// <summary>
    /// Adds a user to our Roles tracking manager class
    /// </summary>
    /// <param name="tableauRole"></param>
    /// <param name="authModel"></param>
    /// <param name="graphUser"></param>
    /// <param name="sourceGroupName"></param>
    private void AddUserToRoleProvisioningTrackingManager(string tableauRole, string authModel, Microsoft.Graph.User graphUser, string sourceGroupName)
    {
        //Because the request code can run async, and the collection management used is not designed to be thread-safe
        //we are going to serialize adding users to the collection.  
        lock(_lock_AddUserToRoleProvisioningTrackingManager)
        {
            AddUserToRoleProvisioningTrackingManager_Inner(tableauRole, authModel, graphUser, sourceGroupName);
        }
    }
    /// <summary>
    /// Add a user to our tracking list
    /// </summary>
    /// <param name="tableauRole"></param>
    /// <param name="graphUser"></param>
    private void AddUserToRoleProvisioningTrackingManager_Inner(string tableauRole, string authModel, Microsoft.Graph.User graphUser, string sourceGroupName)
    {
        string emailCandidate = GetUserEmailFromGraphADUser(graphUser);
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "813-326: User principal name is NULL");

        //Add the user to our tracking set
        SetManagerForRoles.AddUser(new ProvisioningUser(emailCandidate, tableauRole, authModel, sourceGroupName));
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

        GraphServiceClient graphClient = new GraphServiceClient(
            "https://graph.microsoft.com/v1.0/" + _configAzure.AzureAdTenantId,
             new DelegateAuthenticationProvider(async (requestMessage) => {
                 requestMessage.Headers.Authorization = new
                          System.Net.Http.Headers.AuthenticationHeaderValue("bearer", azureSessionToken);
             }));

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

    /*

    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_Warning(string userName, string userRole, string userAuth, string notes)
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
    private void CSVRecord_Error(string userName, string userRole, string userAuth, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area", "user-name", "user-role", "user-auth", "notes" },
            new string[] { "error", userName, userRole, userAuth, notes });
    }
*/
}
