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


    /// <summary>
    /// Makes a starting group for each Group we are provisioning for (so even empty groups get represented, so we know to keep them empty)
    /// </summary>
    private void EnsureTrackingGroupsExist_Groups()
    {
        foreach (var item in _configSyncGroups.GroupsToGroupsSyncList)
        {
            //If the Synchronize group has an explicit output name (i.e. not a wildcard/pattern match),
            //then always create a group for it.  We want to know/show any explicit groups that have 0 members
            var explicitNameOrNull = item.RequiredTargetGroupNameOrNull;
            if (!string.IsNullOrWhiteSpace(explicitNameOrNull))
            {
                this.SetManagerForGroups.EnsureRoleManagerExistsForRole(
                    explicitNameOrNull, 
                    item.GrantLicenseInstructions,
                    item.GrantLicenseRole);
            }
        }
    }

    /// <summary>
    /// Query for each of the specified groups and pull down their members list from Azure
    /// </summary>
    /// <param name="azureSessionToken"></param>
    /// <param name="groupsToRolesSyncList"></param>
    private async Task GenerateGroupsMembersList_FromAzureGroupsSyncList(
        GraphServiceClient azureGraph,
        IEnumerable<ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup> groupsToGroupsSyncList)
    {
        //---------------------------------------------------------------
        //Loop through each of the groups we want to sync
        //---------------------------------------------------------------
        foreach (var groupToRetrieve in groupsToGroupsSyncList)
        {
            _statusLogs.AddStatus("Azure getting group/group sync group(s) for: " + groupToRetrieve.SourceGroupName);

            //Generate the filter command to search of the group
            var azureFilterCommand =
                GenerateAzureMatchCommand(groupToRetrieve.NamePatternMatch, "displayName", groupToRetrieve.SourceGroupName);

            var thisGroupAsSet = await azureGraph.Groups.Request().Select(x => new { x.Id, x.DisplayName }).Filter(azureFilterCommand).GetAsync();
            //----------------------------------------------------------------------------------------------------
            //If the expected group does not exist in Azure, treat the error condition as fatal
            //----------------------------------------------------------------------------------------------------
            if ((thisGroupAsSet.Count < 1) && (groupToRetrieve.NamePatternMatch == ProvisionConfigExternalDirectorySync.NamePatternMatch.Equals))
            {
                _statusLogs.AddError("Azure AD group does not exist: " + groupToRetrieve.SourceGroupName);
                throw new Exception("814-722: Azure AD group does not exist: " + groupToRetrieve.SourceGroupName);
            }

            //----------------------------------------------------------------------------------------------------------------------------------------------
            //If it was a pattern match with no results, just note it and continue (not fatal, because we don't know if the user explicitly expected groups)
            //----------------------------------------------------------------------------------------------------------------------------------------------
            if (thisGroupAsSet.Count < 1)
            {
                _statusLogs.AddStatus("Azure AD has no groups matching the pattern " + groupToRetrieve.SourceGroupName);
            }
            else
            {
                //Process all the groups that are in the results set returned by Azure
                await GenerateGroupsMembersList_ProcessGroups(azureGraph, thisGroupAsSet, groupToRetrieve);
            }
        }
    }

    /// <summary>
    /// Load 1 or more Groups members from Azure and create in memory groups for them
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupAsSet"></param>
    /// <param name="groupToRetrieve"></param>
    /// <returns></returns>
    private async Task GenerateGroupsMembersList_ProcessGroups(
        GraphServiceClient azureGraph, 
        IGraphServiceGroupsCollectionPage thisGroupAsSet, 
        ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup groupToRetrieve)
    {
        //Degenerate case: No data here...
        if ((thisGroupAsSet == null) || (thisGroupAsSet.CurrentPage.Count < 1))
        {
            return;
        }

        //============================================================================================
        //Get all the groups from the current page of Azure results, and then get any subsequent pages
        //============================================================================================
        do
        {
            //Process a;l the groups on this results page
            var currentPage = thisGroupAsSet.CurrentPage;
            var currentPage_ItemCount = currentPage.Count;

            //----------------------------------------------------------------------
            //Loop through all the Azure Groups in the current returned page
            //----------------------------------------------------------------------
            for (var idxGroup = 0; idxGroup < currentPage_ItemCount; idxGroup++)
            {
                await GenerateGroupsMembersList_ProcessSingleGroup(azureGraph, currentPage[idxGroup], groupToRetrieve);
            }

            //-----------------------------------------------------------------------
            //Advance to the next page (if there is one)
            //-----------------------------------------------------------------------
            var requestNextPage = thisGroupAsSet.NextPageRequest;
            if (requestNextPage != null)
            {
                thisGroupAsSet = await requestNextPage.GetAsync();
            }
            else
            {
                thisGroupAsSet = null;
            }

        } while (thisGroupAsSet != null); //Until we have no more Azure Group Pages to look through
    }

    /// <summary>
    /// Process a single top level Azure AD group
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="azureGroup"></param>
    /// <param name="groupSyncInstructions"></param>
    /// <returns></returns>
    private async Task GenerateGroupsMembersList_ProcessSingleGroup(GraphServiceClient azureGraph, Group azureGroup, ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup groupSyncInstructions)
    {
        _statusLogs.AddStatus("Loading members of Azure group '" + azureGroup.DisplayName + "' for sync group '" + groupSyncInstructions.SourceGroupName + "'");

        var thiGroupId = azureGroup.Id;

        //https://docs.microsoft.com/en-us/graph/api/group-list-members?view=graph-rest-1.0&tabs=http
        //UNDONE: Improve performance by filtering down to just USERS and SUB-GROUPS

        var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().GetAsync();
        //TEST: Test paging by forcing  small page size
        //var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().Top(2).GetAsync();

        //Get all the users in the group and sub-groups
        await AzureRecurseGroupsGenerateGroupMembersList(azureGraph, thisGroupsMembers, azureGroup.DisplayName, groupSyncInstructions);
    }

    /// <summary>
    /// Itterate down a groups membership, looking in any sub-groups, and record all the members
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupsMembers"></param>
    /// <param name="groupSyncInstructions"></param>
    async Task AzureRecurseGroupsGenerateGroupMembersList(
        GraphServiceClient azureGraph,
        IGroupMembersCollectionWithReferencesPage thisGroupsMembers,
        string sourceBaseGroupName,
        ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup groupSyncInstructions)
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
                            groupSyncInstructions,
                            sourceBaseGroupName, 
                            asUser);
                        //Add them to the list of users
                    }
                    else if (asSubGroup != null)
                    {
                        //-----------------------------------------------------------------------------------
                        //Recurse down the subgroup and get its members
                        //-----------------------------------------------------------------------------------
                        var subGroupsMembers = await azureGraph.Groups[asSubGroup.Id].Members.Request().GetAsync();
                        await AzureRecurseGroupsGenerateGroupMembersList(azureGraph, subGroupsMembers, sourceBaseGroupName, groupSyncInstructions);
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
    private void AddUserToGroupProvisioningTrackingManager(
        ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup groupSyncInstructions,
        string sourceGroupName,
        Microsoft.Graph.User graphUser)
    {
        //Because the request code can run async, and the collection management used is not designed to be thread-safe
        //we are going to serialize adding users to the collection.  
        lock (_lock_AddUserToGroupProvisioningTrackingManager)
        {
            AddUserToGroupProvisioningTrackingManager_Inner(groupSyncInstructions, sourceGroupName, graphUser);
        }
    }


    /// <summary>
    /// Add the user to the tracking object
    /// </summary>
    /// <param name="targetGroupName"></param>
    /// <param name="graphUser"></param>
    /// <param name="sourceGroupName"></param>
    private void AddUserToGroupProvisioningTrackingManager_Inner(
        ProvisionConfigExternalDirectorySync.ISynchronizeGroupToGroup groupSyncInstructions,
        string sourceGroupName,
        Microsoft.Graph.User graphUser)
    {
        string emailCandidate = GetUserEmailFromGraphADUser(graphUser);
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "843-706: User principal name is NULL");

        //Get the group manager
        var groupManager = SetManagerForGroups.GetManagerForGroup_CreateIfNeeded(
            groupSyncInstructions.GenerateTargetGroupName(sourceGroupName),
            groupSyncInstructions.GrantLicenseInstructions,
            groupSyncInstructions.GrantLicenseRole);

        //Add the user to our tracking set
        groupManager.AddUser(emailCandidate);
    }
}
