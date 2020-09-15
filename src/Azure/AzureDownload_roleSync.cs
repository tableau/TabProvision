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
    private ProvisionFromDirectoryRolesMembershipManager SetManagerForRoles
    {
        get
        {
            return this.ProvisioningManifestResults.RolesManager;
        }
    }

    /// <summary>
    /// Makes a starting group for each Role Group we are provisioning for (so even empty groups get represented, so we know to keep them empty)
    /// </summary>
    private void EnsureTrackingGroupsExist_Roles()
    {
        foreach (var item in _configSyncGroups.GroupsToRolesSyncList)
        {
            this.SetManagerForRoles.EnsureRoleManagerExistsForRole(item.TableauRole);
        }
    }

    /// <summary>
    /// Query for each of the specified groups and pull down their members list from Azure
    /// </summary>
    /// <param name="azureSessionToken"></param>
    /// <param name="groupsToRolesSyncList"></param>
    private async Task GenerateUsersRolesList_FromAzureGroupsSyncList(GraphServiceClient azureGraph, IEnumerable<ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole> groupsToRolesSyncList)
    {
        //Loop through all the Role Sync groups
        foreach (var groupToRetrieve in groupsToRolesSyncList)
        {
            _statusLogs.AddStatus("Azure getting role sync group(s) for: " + groupToRetrieve.SourceGroupName);

            //Generate the filter command to search of the group
            var azureFilterCommand =
                GenerateAzureMatchCommand(groupToRetrieve.NamePatternMatch, "displayName", groupToRetrieve.SourceGroupName);
            //Query for it
            var thisGroupAsSet = await azureGraph.Groups.Request().Select(x => new { x.Id, x.DisplayName }).Filter(azureFilterCommand).GetAsync();


            //----------------------------------------------------------------------------------------------------
            //If the expected group does not exist in Azure, treat the error condition as fatal
            //----------------------------------------------------------------------------------------------------
            if ((thisGroupAsSet.Count < 1) && (groupToRetrieve.NamePatternMatch == ProvisionConfigExternalDirectorySync.NamePatternMatch.Equals))
            {
                _statusLogs.AddError("Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
                throw new Exception("814-723: Azure AD group does not exist" + groupToRetrieve.SourceGroupName);
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
                await GenerateUsersRolesListFromAzureGroups_ProcessGroups(azureGraph, thisGroupAsSet, groupToRetrieve);
            }
        } //next group-sync instruction
    }

    /// <summary>
    /// Process all the Groups in the set.  Get the groups members (recursively) and add them to the right Role buckets
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupAsSet"></param>
    /// <param name="groupToRetrieve"></param>
    /// <returns></returns>
    private async Task GenerateUsersRolesListFromAzureGroups_ProcessGroups(
        GraphServiceClient azureGraph,
        IGraphServiceGroupsCollectionPage thisGroupAsSet,
        ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole groupToRetrieve)
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
            //----------------------------------------------------------------------
            //Loop through all the Azure Groups in the current returned page
            //----------------------------------------------------------------------
            var currentPage = thisGroupAsSet.CurrentPage;
            var currentPage_ItemCount = currentPage.Count;
            for (var idxGroup = 0; idxGroup < currentPage_ItemCount; idxGroup++)
            {
                await GenerateUsersRolesListFromAzureGroups_ProcessSingleGroup(azureGraph, currentPage[idxGroup], groupToRetrieve);
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

        } while (thisGroupAsSet != null);
    }

    /// <summary>
    /// Get the group's members (recursively) and add them to the right Role buckets
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupAsSet"></param>
    /// <param name="groupToRetrieve"></param>
    /// <returns></returns>

    private async Task GenerateUsersRolesListFromAzureGroups_ProcessSingleGroup(GraphServiceClient azureGraph, Group azureGroup, ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole groupToRetrieve)
    {
        _statusLogs.AddStatus("Get Azure AD group membership from '" + azureGroup.DisplayName + "' for user-role mapping for group '" + groupToRetrieve.SourceGroupName + "'");

        //----------------------------------------------------------------------------------------------------
        //Get all the members of the group
        //----------------------------------------------------------------------------------------------------
        var thiGroupId = azureGroup.Id;

        //https://docs.microsoft.com/en-us/graph/api/group-list-members?view=graph-rest-1.0&tabs=http
        //UNDONE: Filter down to just USERS and SUB-GROUPS

        var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().GetAsync();

        //TEST: Test paging by forcing the # of items to be returned per page to be 2
        //var thisGroupsMembers = await azureGraph.Groups[thiGroupId].Members.Request().Top(2).GetAsync();
        //Get all the users in the group and sub-groups
        await AzureRecurseGroupsGenerateRolesList(azureGraph, thisGroupsMembers, groupToRetrieve);
    }

    /// <summary>
    /// Itterate down a groups membership, looing in any sub-groups, and record all the members
    /// </summary>
    /// <param name="azureGraph"></param>
    /// <param name="thisGroupsMembers"></param>
    /// <param name="baseGroupToRetrieve"></param>
    async Task AzureRecurseGroupsGenerateRolesList(GraphServiceClient azureGraph, IGroupMembersCollectionWithReferencesPage thisGroupsMembers, ProvisionConfigExternalDirectorySync.SynchronizeGroupToRole baseGroupToRetrieve)
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
                            baseGroupToRetrieve.AllowPromotedRoleForMembers,
                            baseGroupToRetrieve.AuthenticationModel,
                            asUser,
                            baseGroupToRetrieve.SourceGroupName);
                        //Add them to the list of users
                    }
                    else if (asSubGroup != null)
                    {
                        //-----------------------------------------------------------------------------------
                        //Recurse down the subgroup and get its members
                        //-----------------------------------------------------------------------------------
                        var subGroupsMembers = await azureGraph.Groups[asSubGroup.Id].Members.Request().GetAsync();
                        await AzureRecurseGroupsGenerateRolesList(azureGraph, subGroupsMembers, baseGroupToRetrieve);
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
    private void AddUserToRoleProvisioningTrackingManager(string tableauRole, bool allowPromotedRole, string authModel, Microsoft.Graph.User graphUser, string sourceGroupName)
    {
        //Because the request code can run async, and the collection management used is not designed to be thread-safe
        //we are going to serialize adding users to the collection.  
        lock (_lock_AddUserToRoleProvisioningTrackingManager)
        {
            AddUserToRoleProvisioningTrackingManager_Inner(tableauRole, allowPromotedRole, authModel, graphUser, sourceGroupName);
        }
    }
    /// <summary>
    /// Add a user to our tracking list
    /// </summary>
    /// <param name="tableauRole"></param>
    /// <param name="graphUser"></param>
    private void AddUserToRoleProvisioningTrackingManager_Inner(string tableauRole, bool allowPromotedRole, string authModel, Microsoft.Graph.User graphUser, string sourceGroupName)
    {
        string emailCandidate = GetUserEmailFromGraphADUser(graphUser);
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(emailCandidate), "813-326: User principal name is NULL");

        //Add the user to our tracking set
        SetManagerForRoles.AddUser(new ProvisioningUser(emailCandidate, tableauRole, authModel, sourceGroupName, allowPromotedRole));
    }


}
