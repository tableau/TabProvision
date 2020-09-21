using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provision a site
/// </summary>
internal partial class ProvisionSite
{
    /// <summary>
    /// Provision the groups
    /// </summary>
    /// <param name="siteSignIn"></param>
    private void Execute_ProvisionGroups(TableauServerSignIn siteSignIn, WorkingListSiteUsers workingList_allKnownUsers)
    {
        _statusLogs.AddStatusHeader("Provision the specified groups in site");

        //=================================================================================
        //If we do not have it already, load the set of users for the site...we will need this to look up users
        //=================================================================================
        if (workingList_allKnownUsers == null)
        {
            var existingUsers = DownloadUsersList.CreateAndExecute(siteSignIn);
            existingUsers.ExecuteRequest();

            workingList_allKnownUsers = new WorkingListSiteUsers(existingUsers.Users);
        }

        //=================================================================================
        //Download the groups
        //=================================================================================
        var downloadGroups = new DownloadGroupsList(siteSignIn);
        downloadGroups.ExecuteRequest(false); //Download the list of groups, but not the membership of the groups (we will only do that for the groups we care about)

        //Go through each of the groups...
        foreach (var thisProvisionGroup in _provisionInstructions.GroupsToProvision)
        {
            Execute_ProvisionGroups_SingleGroup(siteSignIn, thisProvisionGroup, downloadGroups, workingList_allKnownUsers);
        }
    }

    /// <summary>
    /// Provisioning for a single group
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="thisProvisionGroup"></param>
    /// <param name="existingGroups"></param>
    private void Execute_ProvisionGroups_SingleGroup(
        TableauServerSignIn siteSignIn,
        ProvisioningGroup thisProvisionGroup,
        DownloadGroupsList existingGroups,
        WorkingListSiteUsers siteUsersList)
    {
        _statusLogs.AddStatusHeader("Provision the group: " + thisProvisionGroup.GroupName);

        var thisExistingGroup = existingGroups.FindGroupWithName(thisProvisionGroup.GroupName);
        ICollection<SiteUser> existingUsersInGroup = new List<SiteUser>();

        //If the Group does not exist on server then create it
        if (thisExistingGroup == null)
        {
            var createGroup = new SendCreateGroup(siteSignIn, thisProvisionGroup.GroupName);
            thisExistingGroup = createGroup.ExecuteRequest();

            CSVRecord_GroupModified(thisExistingGroup.Name, "created group", "");
            _statusLogs.AddStatus("Created group: " + thisExistingGroup.Name);

            //--------------------------------------------------------------------------
            //Do we need to update the group's Grant License?
            //--------------------------------------------------------------------------
            Execute_ProvisionGroups_SingleGroup_GrantLicenseUpdate(siteSignIn, thisProvisionGroup, thisExistingGroup);

        }
        else
        {
            //Download the members of the group
            var downloadGroupMembers = new DownloadUsersListInGroup(siteSignIn, thisExistingGroup.Id);
            downloadGroupMembers.ExecuteRequest();
            existingUsersInGroup = downloadGroupMembers.Users;

            //--------------------------------------------------------------------------
            //Do we need to update the group's Grant License?
            //--------------------------------------------------------------------------
            Execute_ProvisionGroups_SingleGroup_GrantLicenseUpdate(siteSignIn, thisProvisionGroup, thisExistingGroup);
        }



        //====================================================================================
        //Keep a list of the remaining users in the Server Site's group
        //====================================================================================
        var workingListUnexaminedUsers = new WorkingListSiteUsers(existingUsersInGroup);

        //====================================================================================
        //Go through each of the users we need to provision, and see if they are in the group 
        //already
        //====================================================================================
        foreach (var provisionThisUser in thisProvisionGroup.Members)
        {
            var userInGroup = workingListUnexaminedUsers.FindUserByName(provisionThisUser);
            if (userInGroup != null)
            {
                //The user is already in the group, no need to add them
                workingListUnexaminedUsers.RemoveUser(userInGroup);
            }
            else
            {

                //Add the user to the group
                try
                {
                    Execute_ProvisionGroups_SingleGroup_AddUser(siteSignIn, provisionThisUser, thisExistingGroup, siteUsersList);
                }
                catch(Exception exAddUserToGroup) //Unexpected error case
                {
                    IwsDiagnostics.Assert(false, "811-700: Internal error adding user to group: " + exAddUserToGroup.Message);
                    _statusLogs.AddError("811-700: Internal error adding user to group: " + exAddUserToGroup.Message);
                }

            }
        }

        //==============================================================================
        //Remove any remaining users that are in the Server Site's Group but not in
        //our provisioning list
        //==============================================================================
        foreach (var unexpectedUser in workingListUnexaminedUsers.GetUsers())
        {
            try
            {
                Execute_ProvisionGroups_RemoveSingleUser(siteSignIn, unexpectedUser, thisExistingGroup);
            }
            catch (Exception exUnxpectedUsers)
            {
                _statusLogs.AddError("Error removing unexpected user in GROUP " + unexpectedUser.ToString() + ", " + exUnxpectedUsers.Message);
                CSVRecord_ErrorUpdatingUser(unexpectedUser.Name, unexpectedUser.SiteRole, unexpectedUser.SiteAuthentication, "Error removing unexpected user in GROUP" + unexpectedUser.ToString() + ", " + exUnxpectedUsers.Message);
            }
        }

    }


    /// <summary>
    /// See if we need to update the Grant License on Sign In behavior of a group
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="groupProvisionInstructions"></param>
    /// <param name="thisExistingGroup"></param>
    private void Execute_ProvisionGroups_SingleGroup_GrantLicenseUpdate(
        TableauServerSignIn siteSignIn, 
        ProvisioningGroup groupProvisionInstructions, 
        SiteGroup thisExistingGroup)
    {
        //If are instruction is to INGORE this attribute, then there is nothing to do
        if(groupProvisionInstructions.GrantLicenseInstructions == ProvisioningGroup.GrantLicenseMode.Ignore)
        {
            return;
        }

        //======================================================================================
        //Determine if we need to updae the group's Grant License setting 
        //======================================================================================
        string newGrantLicensingMode;
        switch(groupProvisionInstructions.GrantLicenseInstructions)
        {
            case ProvisioningGroup.GrantLicenseMode.None:
               //If the provisioning instructions are for NO Grant Licensing mode, and this is already the case, then there is nothing to do
               if (string.IsNullOrEmpty(thisExistingGroup.GrantLicenseMode)) 
                    { return; }

                newGrantLicensingMode = ""; //Set the new mode as blank;
                break;

            case ProvisioningGroup.GrantLicenseMode.OnLogin:
                //If the grant license mode and ROLEis already as expected, then there is nothing to do
                if ((thisExistingGroup.GrantLicenseMode == ProvisioningGroup.GrantLicenseMode_OnLogin) 
                    && (thisExistingGroup.SiteMinimumRoleOrNull == groupProvisionInstructions.GrantLicenseRole))
                    { return; }

                newGrantLicensingMode = ProvisioningGroup.GrantLicenseMode_OnLogin; //Set the new mode;
                break;

            //Degenerate case: Unknown instruction
            default:
                IwsDiagnostics.Assert(false, "920-1130: Unknown Grant License instruction for group: " + groupProvisionInstructions.GroupName);
                throw new Exception("920-1130: Unknown Grant License instruction for group: " + groupProvisionInstructions.GroupName);
        }

        //================================================================================
        //Perform the update to the site
        //================================================================================
        var sendUpdateGroup = new SendUpdateGroup(
            siteSignIn,
            thisExistingGroup.Id,
            groupProvisionInstructions.GroupName,
            true,
            newGrantLicensingMode,
            groupProvisionInstructions.GrantLicenseRole);

        var wasSuccessUpdatingGroup = false;
        string updateGroupErrorText = "";

        try
        {
            wasSuccessUpdatingGroup = sendUpdateGroup.ExecuteRequest();
        }
        catch(Exception exUpdateGroup)
        {
            updateGroupErrorText = exUpdateGroup.Message;
        }

        if(!wasSuccessUpdatingGroup)
        {
            //Record failure
            CSVRecord_ErrorUpdatingGroup(groupProvisionInstructions.GroupName,
                "Error attempting to update group auto grant roles: "
                + NullSafeText(thisExistingGroup.SiteMinimumRoleOrNull) 
                + "->" + NullSafeText(groupProvisionInstructions.GrantLicenseRole) +
                ", error details: " + updateGroupErrorText);
        }
        else
        {
            //Record success
            CSVRecord_GroupModified(
                groupProvisionInstructions.GroupName
                , "Grant license: " + NullSafeText(thisExistingGroup.GrantLicenseMode) + "->" + NullSafeText(newGrantLicensingMode)
                  + ", Site role: " + NullSafeText(thisExistingGroup.SiteMinimumRoleOrNull) + "->" + NullSafeText(groupProvisionInstructions.GrantLicenseRole)
                , ""
                );
        }

    }

    /// <summary>
    /// Simple helper
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string NullSafeText(string text)
    {
        if(string.IsNullOrEmpty(text))
        {
            return "";
        }
        return text;
    }

    /// <summary>
    /// REMOVE an undesired user from a group
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="userRemoveFromGroup"></param>
    /// <param name="siteGroup"></param>
    private void Execute_ProvisionGroups_RemoveSingleUser(TableauServerSignIn siteSignIn, SiteUser userRemoveFromGroup, SiteGroup siteGroup)
    {
        _statusLogs.AddStatus("Attempting to remove the user " + userRemoveFromGroup.Name + " from group " + siteGroup.Name);


        switch (_provisionInstructions.ActionForUnexpectedGroupMembers)
        {
            case ProvisionUserInstructions.UnexpectedGroupMemberAction.Delete:
                var deleteUserFromGroup = new SendDeleteUserFromGroup(siteSignIn, userRemoveFromGroup.Id, siteGroup.Id);
                bool wasSuccess = deleteUserFromGroup.ExecuteRequest();

                if (wasSuccess)
                {
                    //SUCCESS
                    CSVRecord_GroupModified_WithUser(siteGroup.Name, "removed member", userRemoveFromGroup.Name, "");
                    _statusLogs.AddStatus("Group membership: Removed " + userRemoveFromGroup.Name + " from group " + siteGroup.Name);
                }
                else
                {
                    CSVRecord_ErrorUpdatingUser(userRemoveFromGroup.Name, "", "", "User could not be removed to group " + siteGroup.Name);
                    _statusLogs.AddError("Group membership error: Failed to remove " + userRemoveFromGroup.Name + " to group " + siteGroup.Name);
                }

                return;

            case ProvisionUserInstructions.UnexpectedGroupMemberAction.Report:
                //We are instructed to NOT REALLY DELETE the user from the group, jsut report
                CSVRecord_GroupModified_WithUser(siteGroup.Name, "SIMULATED removed member", userRemoveFromGroup.Name, "");
                return;

            default:
                IwsDiagnostics.Assert(false, "814-433: Unknown action");
                throw new Exception("814-433: Unknown action");

        }
    }

    /// <summary>
    /// Add a specific user into the Server Site's group
    /// </summary>
    /// <param name="userEmail"></param>
    /// <param name="thisProvisionGroup"></param>
    /// <param name="siteUsersList"></param>
    private void Execute_ProvisionGroups_SingleGroup_AddUser(
        TableauServerSignIn siteSignIn, 
        string userEmail, 
        SiteGroup siteGroup, 
        WorkingListSiteUsers siteUsersList)
    {
        var siteUserToAddToGroup = siteUsersList.FindUserByName(userEmail);
        //Sanity test. If the user is not a member of the site, they cannot be added to a group
        if (siteUserToAddToGroup == null)
        {
            CSVRecord_ErrorUpdatingUser(userEmail, "", "", "User not on site. Cannot be added to group");
            _statusLogs.AddError("User not on site. Cannot be added to group, " + userEmail);
            return; //FAILED
        }

        switch(_provisionInstructions.ActionForMissingGroupMembers)
        {
            case ProvisionUserInstructions.MissingGroupMemberAction.Add:
                //Call the server and add the user
                var addUserToGroup = new SendAddUserToGroup(siteSignIn, siteUserToAddToGroup.Id, siteGroup.Id);
                bool userGroupAddSuccess = addUserToGroup.ExecuteRequest();
                if (userGroupAddSuccess)
                {
                    //SUCCESS
                    CSVRecord_GroupModified_WithUser(siteGroup.Name, "added member", siteUserToAddToGroup.Name, "");
                    _statusLogs.AddStatus("Group membership: Added " + siteUserToAddToGroup.Name + " to group " + siteGroup.Name);
                }
                else
                {
                    CSVRecord_ErrorUpdatingUser(userEmail, "", "", "User could not be added to group " + siteGroup.Name);
                    _statusLogs.AddError("Group membership error: Failed to add " + siteUserToAddToGroup.Name + " to group " + siteGroup.Name);
                }
                return;

                
            case ProvisionUserInstructions.MissingGroupMemberAction.Report:
                //We are instructed to NOT REALLY add the user, jsut report
                CSVRecord_GroupModified_WithUser(siteGroup.Name, "SIMULATED added member", siteUserToAddToGroup.Name, "");
                return;

            default:
                IwsDiagnostics.Assert(false, "814-433: Unknown action");
                throw new Exception("814-433: Unknown action");

        }

    }


    /// <summary>
    /// Make a record of a group modification
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_GroupModified(string groupName, string modification, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area"              , "group-name", "modification", "notes" },
            new string[] { "group provisioning",  groupName  , modification  ,  notes });
    }

    /// <summary>
    /// Make a record of a group modification
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_GroupModified_WithUser(string groupName, string modification, string userName, string notes)
    {
        _csvProvisionResults.AddKeyValuePairs(
            new string[] { "area", "group-name", "modification", "user-name", "notes" },
            new string[] { "group provisioning", groupName, modification, userName, notes });
    }

}


