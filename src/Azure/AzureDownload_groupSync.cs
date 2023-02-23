using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Graph;
using System.Linq;

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
            // Abhinav Pathak : Added a condition if Account name and Display Name is not same
            //----------------------------------------------------------------------------------------------------

            if (thisGroupAsSet.Count == 0)
            {
                azureFilterCommand =
                GenerateAzureMatchCommand(groupToRetrieve.NamePatternMatch, "mailNickname", groupToRetrieve.SourceGroupName);

                thisGroupAsSet = await azureGraph.Groups.Request().Select(x => new { x.Id, x.DisplayName, x.OnPremisesSamAccountName, x.MailNickname }).Filter(azureFilterCommand).GetAsync();

            }

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
        //----------------------------------------------------------------------------------------------------------------------------------------------
        //See if there is an additional 'contains' filter we need to apply to the result
        //----------------------------------------------------------------------------------------------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(groupSyncInstructions.FilterSourceGroupNameContains))
        {
            //If the Azure Group does not contain the specified Contains fitering term, then skip it
            if (!azureGroup.DisplayName.Contains(groupSyncInstructions.FilterSourceGroupNameContains))
            {
                _statusLogs.AddStatus("Skipping groups sync members of group: '"
                    + azureGroup.DisplayName
                    + "', becuase the group name does not contain the filter term '"
                    + groupSyncInstructions.FilterSourceGroupNameContains
                    + "'");
                return;
            }
        }

        _statusLogs.AddStatus("Loading members of Azure group '" + azureGroup.DisplayName + "' for sync group '" + groupSyncInstructions.SourceGroupName + "'");

        //==============================================================
        //Get/Create the membership manager for the group
        //==============================================================
        var singleGroupMembershipManager = SetManagerForGroups.GetManagerForGroup_CreateIfNeeded(
            groupSyncInstructions.GenerateTargetGroupName(azureGroup.DisplayName),
            groupSyncInstructions.GrantLicenseInstructions,
            groupSyncInstructions.GrantLicenseRole);

        var thiGroupId = azureGroup.Id;

        //https://docs.microsoft.com/en-us/graph/api/group-list-members?view=graph-rest-1.0&tabs=http
        //UNDONE: Improve performance by filtering down to just USERS and SUB-GROUPS

        var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().GetAsync();
        //TEST: Test paging by forcing  small page size
        //var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().Top(2).GetAsync();

        //Get all the users in the group and sub-groups
        await AzureRecurseGroupsGenerateGroupMembersList(azureGraph, thisGroupsMembers, singleGroupMembershipManager);
    }

    /// <summary>
    /// Itterate down a groups membership, looking in any sub-groups, and record all the members
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupsMembers"></param>
    /// <param name="singleGroupMembershipManager"></param>
    /// <param name="groupSyncInstructions"></param>
    /// <returns></returns>
    async Task AzureRecurseGroupsGenerateGroupMembersList(
        GraphServiceClient azureGraph,
        IGroupMembersCollectionWithReferencesPage thisGroupsMembers,
        ProvisionFromDirectoryGroupsMembershipManager.SingleGroupManager singleGroupMembershipManager)
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
                    //-------------------------------------------------
                    //If it's a USER, then add it to our tracking list
                    //-------------------------------------------------
                    if (asUser != null)
                    {
                        //-------------------------------------------------
                        //Add them to the list of users
                        //-------------------------------------------------
                        AddUserToGroupProvisioningTrackingManager(
                            singleGroupMembershipManager, 
                            asUser);
                    }
                    //-------------------------------------------------
                    //If it's a GROUP, then recurse down it
                    //-------------------------------------------------
                    else if (asSubGroup != null)
                    {
                        //-----------------------------------------------------------------------------------
                        //Recurse down the subgroup and get its members
                        //-----------------------------------------------------------------------------------
                        var subGroupsMembers = await azureGraph.Groups[asSubGroup.Id].Members.Request().GetAsync();
                        await AzureRecurseGroupsGenerateGroupMembersList(azureGraph, subGroupsMembers, singleGroupMembershipManager);
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
    /// Add a user to specified Group 
    /// </summary>
    /// <param name="groupSyncInstructions"></param>
    /// <param name="singleGroupMembershipManager"></param>
    /// <param name="graphUser"></param>
    private void AddUserToGroupProvisioningTrackingManager(
        ProvisionFromDirectoryGroupsMembershipManager.SingleGroupManager singleGroupMembershipManager,
        Microsoft.Graph.User graphUser)
    {
        string emailCandidate = GetUserEmailFromGraphADUser(graphUser);
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "843-706: User principal name is NULL");

        singleGroupMembershipManager.AddUser(emailCandidate);
    }


}
