using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provision a site
/// </summary>
internal partial class ProvisionSite
{

    /// <summary>
    /// Follows the instructions for any content ownership changes we want to make
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="workingList_allKnownUsers"></param>
    private void Execute_ProvisionContentOwnershipChanges(TableauServerSignIn siteSignIn, WorkingListSiteUsers workingList_allKnownUsers)
    {

        //===============================================================================================
        //If there are no content ownership changes to perform, then just exit this step
        //===============================================================================================
        if ((_provisionInstructions.ContentOwnershipToProvision == null) ||
          (_provisionInstructions.ContentOwnershipToProvision.Count == 0))
        {
            _statusLogs.AddStatus("Skipping content ownership mapping, because there are no instructions for this...");
            return;
        }


        _statusLogs.AddStatusHeader("Provision content ownership changes");

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
        //Unlike Workbooks, there is not a simple URL to download the list of Datasources 
        //for a single user.  (There is a filter parameter that can be added to the REST
        //API that queries data sources, but that takes a "user name" not a User ID).
        //Here we can query for ALL the data sources pass that to all teh remappign request
        //=================================================================================
        var downloadDataSources = new DownloadDatasourcesList(siteSignIn);
        downloadDataSources.ExecuteRequest();
        var allSiteDataSources = downloadDataSources.Datasources;

        //=================================================================================
        //Get the list of flows in the iste
        //=================================================================================
        var downloadFlows = new DownloadFlowsList(siteSignIn);
        downloadFlows.ExecuteRequest();
        var allSiteFlows = downloadFlows.Flows;

        //Go through each of the ownership change instructions...
        foreach (var thisOwnershipChange in _provisionInstructions.ContentOwnershipToProvision)
        {
            Execute_ProvisionOwnership_SingleUserChange(
                siteSignIn,
                thisOwnershipChange, workingList_allKnownUsers,
                allSiteDataSources,
                allSiteFlows);
        }

    }


    /// <summary>
    /// Change the ownership from 1 user, to another user
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="thisOwnershipChange"></param>
    /// <param name="workingList_allKnownUsers"></param>
    private void Execute_ProvisionOwnership_SingleUserChange(
        TableauServerSignIn siteSignIn, 
        ProvisioningChangeContentOwnership thisOwnershipChange, 
        WorkingListSiteUsers workingList_allKnownUsers,
        ICollection<SiteDatasource> knownDataSources,
        ICollection<SiteFlow> knownFlows)
    {
        try
        {
            Execute_ProvisionOwnership_SingleUserChange_inner(
                siteSignIn, 
                thisOwnershipChange, 
                workingList_allKnownUsers,
                knownDataSources,
                knownFlows);
        }
        catch(Exception ex)
        {
            _statusLogs.AddError("Error attempting to change content ownership, from: " 
                + thisOwnershipChange.OldOwnerName 
                + ", to:" + thisOwnershipChange.NewOwnerName
                + ", error: " + ex.ToString());
        }
    }

    /// <summary>
    /// Process the ownership-mapping request for a single user
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="thisOwnershipChange"></param>
    /// <param name="workingList_allKnownUsers"></param>
    /// <param name="knownDataSources"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_inner(
        TableauServerSignIn siteSignIn, 
        ProvisioningChangeContentOwnership thisOwnershipChange, 
        WorkingListSiteUsers workingList_allKnownUsers,
        ICollection<SiteDatasource> knownDataSources,
        ICollection<SiteFlow> knownFlows)
    {
        var userOldOwner = workingList_allKnownUsers.FindUserByName(thisOwnershipChange.OldOwnerName);
        if(userOldOwner == null)
        {
            throw new Exception("201202-415: Could not find user: " + userOldOwner.Name);
        }

        var userNewOwner = workingList_allKnownUsers.FindUserByName(thisOwnershipChange.NewOwnerName);
        if (userNewOwner == null)
        {
            throw new Exception("201202-416: Could not find user: " + userNewOwner.Name);
        }


        //------------------------------------------------------------------------------------
        //Check the ownership of each of these types of content, and update from the 
        //old owner to the designated new owner
        //------------------------------------------------------------------------------------
        Execute_ProvisionOwnership_SingleUserChange_Workbooks(siteSignIn, userOldOwner, userNewOwner);
        Execute_ProvisionOwnership_SingleUserChange_Datasources(siteSignIn, knownDataSources, userOldOwner, userNewOwner);
        Execute_ProvisionOwnership_SingleUserChange_Flows(siteSignIn, userOldOwner, userNewOwner, knownFlows);
    }



    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_ErrorUpdatingContentOwnership(string contentType, string contentName, string oldOwnerName, string newOwnerName, string errorNotes)
    {
        var colNames = new List<string>();
        var colValues = new List<string>();

        colNames.Add("area");            colValues.Add("error");
        colNames.Add("content-name");    colValues.Add(contentName);
        colNames.Add("user-name");       colValues.Add(newOwnerName);
        colNames.Add("modification");    colValues.Add("ownership change");
        colNames.Add("notes");           colValues.Add("Error updating ownership: " + errorNotes + ". Previous owner was: " + oldOwnerName);

        _csvProvisionResults.AddKeyValuePairs(colNames.ToArray(), colValues.ToArray());
    }


    /// <summary>
    /// Make a record of a user modification
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuth"></param>
    /// <param name="modification"></param>
    /// <param name="notes"></param>
    private void CSVRecord_ContentOwnershipModified(string contentType, string contentName, string oldOwnerName, string newOwnerName)
    {
        var colNames = new List<string>();
        var colValues = new List <string>();

        colNames.Add("area");         colValues.Add(contentType);
        colNames.Add("content-name"); colValues.Add(contentName);
        colNames.Add("user-name");    colValues.Add(newOwnerName);
        colNames.Add("modification"); colValues.Add("ownership change");
        colNames.Add("notes");        colValues.Add("previous owner was: " + oldOwnerName);

        _csvProvisionResults.AddKeyValuePairs(colNames.ToArray(), colValues.ToArray());
    }

}


