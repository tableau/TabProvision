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
    /// <param name="thisOwnershipChange"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_Workbooks(TableauServerSignIn siteSignIn, SiteUser userOldOwner, SiteUser userNewOwner)
    {
        //Get the list of workbooks (if any) owned by the old user
        var inventoryDownloader = new DownloadWorkbooksList(siteSignIn, userOldOwner.Id, true);
        inventoryDownloader.ExecuteRequest();
        var listContent = inventoryDownloader.Workbooks;

        if ((listContent == null) || (listContent.Count < 1))
        {
            _statusLogs.AddStatus("No work to do. No workbooks owned by: " + userOldOwner.Name);
            return;
        }

        //Change the ownership of each of these content items
        foreach (var contentItem in listContent)
        {

            if (string.Compare(contentItem.OwnerId, userOldOwner.Id, true) == 0)
            {
                Execute_ProvisionOwnership_SingleUserChange_SingleWorkbook(siteSignIn, contentItem, userOldOwner, userNewOwner);
            }
            else
            {
                IwsDiagnostics.Assert(false, "201202-607: Expected not to have content not owned by user");
            }
        }
    }


    /// <summary>
    /// Change the ownership for a single Workbook
    /// </summary>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleWorkbook(TableauServerSignIn siteSignIn, SiteWorkbook contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {
        try
        {
            Execute_ProvisionOwnership_SingleUserChange_SingleWorkbook_inner(siteSignIn, contentItem, userOldOwner, userNewOwner);
        }
        catch (Exception ex)
        {
            _statusLogs.AddError("Error attempting to change content ownership, "
                + "workbook: " + contentItem.Name
                + "from: " + userOldOwner.Name
                + ", to:" + userNewOwner.Name
                + ", error: " + ex.ToString());

            CSVRecord_ErrorUpdatingContentOwnership("workbook", contentItem.Name, userOldOwner.Name, userNewOwner.Name, ex.Message);
        }
    }

    /// <summary>
    /// Update the owner of a single workbook
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleWorkbook_inner(TableauServerSignIn siteSignIn, SiteWorkbook contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {

        var updateContentOwner = new SendUpdateWorkbookOwner(siteSignIn, contentItem.Id, userNewOwner.Id);
        updateContentOwner.ExecuteRequest();

        CSVRecord_ContentOwnershipModified("workbook", contentItem.Name, userOldOwner.Name, userNewOwner.Name);

    }


}


