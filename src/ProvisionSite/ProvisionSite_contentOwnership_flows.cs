using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provision a site
/// </summary>
internal partial class ProvisionSite
{

    /// <summary>
    /// For this type of content, change the ownership from one user to another
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    /// <param name="knownFlows"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_Flows(
        TableauServerSignIn siteSignIn, 
        SiteUser userOldOwner, 
        SiteUser userNewOwner,
        ICollection<SiteFlow> knownFlows)
    {

        //Change the ownership of each of these content items
        foreach (var contentItem in knownFlows)
        {

            if (string.Compare(contentItem.OwnerId, userOldOwner.Id, true) == 0)
            {
                Execute_ProvisionOwnership_SingleUserChange_SingleFlow(siteSignIn, contentItem, userOldOwner, userNewOwner);
            }
            else
            {
                //Nothing to do. The flow does nto belong to the user
            }
        }
    }


    /// <summary>
    /// Change the ownership for a single Flow
    /// </summary>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleFlow(TableauServerSignIn siteSignIn, SiteFlow contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {
        try
        {
            Execute_ProvisionOwnership_SingleUserChange_SingleFlow_inner(siteSignIn, contentItem, userOldOwner, userNewOwner);
        }
        catch (Exception ex)
        {
            _statusLogs.AddError("Error attempting to change content ownership, "
                + "flow: " + contentItem.Name
                + "from: " + userOldOwner.Name
                + ", to:" + userNewOwner.Name
                + ", error: " + ex.ToString());

            CSVRecord_ErrorUpdatingContentOwnership("flow", contentItem.Name, userOldOwner.Name, userNewOwner.Name, ex.Message);
        }
    }

    /// <summary>
    /// Update the owner of a single flow
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleFlow_inner(TableauServerSignIn siteSignIn, SiteFlow contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {

        var updateContentOwner = new SendUpdateFlowOwner(siteSignIn, contentItem.Id, userNewOwner.Id);
        updateContentOwner.ExecuteRequest();

        CSVRecord_ContentOwnershipModified("flow", contentItem.Name, userOldOwner.Name, userNewOwner.Name);

    }

    /* Keep this earlier implementation around for REST API debugging.
/// <summary>
/// For this type of content, change the ownership from one user to another
/// </summary>
/// <param name="siteSignIn"></param>
/// <param name="thisOwnershipChange"></param>
/// <param name="userOldOwner"></param>
/// <param name="userNewOwner"></param>
private void Execute_ProvisionOwnership_SingleUserChange_Flows(TableauServerSignIn siteSignIn, SiteUser userOldOwner, SiteUser userNewOwner)
{
    //Get the list of flows (if any) owned by the old user
    var inventoryDownloader = new DownloadFlowsList(siteSignIn, userOldOwner.Id, true);
    inventoryDownloader.ExecuteRequest();
    var listContent = inventoryDownloader.Flows;

    if ((listContent == null) || (listContent.Count < 1))
    {
        _statusLogs.AddStatus("No work to do. No flows owned by: " + userOldOwner.Name);
        return;
    }

    //Change the ownership of each of these content items
    foreach (var contentItem in listContent)
    {

        if (string.Compare(contentItem.OwnerId, userOldOwner.Id, true) == 0)
        {
            Execute_ProvisionOwnership_SingleUserChange_SingleFlow(siteSignIn, contentItem, userOldOwner, userNewOwner);
        }
        else
        {
            IwsDiagnostics.Assert(false, "201202-607: Expected not to have content not owned by user");
        }
    }
}
*/


}


