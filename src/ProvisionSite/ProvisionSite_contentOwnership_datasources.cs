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
    private void Execute_ProvisionOwnership_SingleUserChange_Datasources(TableauServerSignIn siteSignIn, ICollection<SiteDatasource> knownDataSources, SiteUser userOldOwner, SiteUser userNewOwner)
    {
        //If there are no known datasources in the site, there is nothing to do
        if((knownDataSources == null) || (knownDataSources.Count == 0))
        {
            return;
        }

        //Change the ownership of each of these content items
        foreach (var contentItem in knownDataSources)
        {

            if (string.Compare(contentItem.OwnerId, userOldOwner.Id, true) == 0)
            {
                Execute_ProvisionOwnership_SingleUserChange_SingleDatasource(siteSignIn, contentItem, userOldOwner, userNewOwner);
            }
            else
            {
                //The datasouce is not owned by the user, there is nothing to do...
            }
        }
    }


    /// <summary>
    /// Change the ownership for a single Datasource
    /// </summary>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleDatasource(TableauServerSignIn siteSignIn, SiteDatasource contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {
        try
        {
            Execute_ProvisionOwnership_SingleUserChange_SingleDatasource_inner(siteSignIn, contentItem, userOldOwner, userNewOwner);
        }
        catch (Exception ex)
        {
            _statusLogs.AddError("Error attempting to change content ownership, "
                + "datasource: " + contentItem.Name
                + "from: " + userOldOwner.Name
                + ", to:" + userNewOwner.Name
                + ", error: " + ex.ToString());

            CSVRecord_ErrorUpdatingContentOwnership("datasource", contentItem.Name, userOldOwner.Name, userNewOwner.Name, ex.Message);
        }
    }

    /// <summary>
    /// Update the owner of a single datasource
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <param name="contentItem"></param>
    /// <param name="userOldOwner"></param>
    /// <param name="userNewOwner"></param>
    private void Execute_ProvisionOwnership_SingleUserChange_SingleDatasource_inner(TableauServerSignIn siteSignIn, SiteDatasource contentItem, SiteUser userOldOwner, SiteUser userNewOwner)
    {

        var updateContentOwner = new SendUpdateDatasourceOwner(siteSignIn, contentItem.Id, userNewOwner.Id);
        updateContentOwner.ExecuteRequest();

        CSVRecord_ContentOwnershipModified("datasource", contentItem.Name, userOldOwner.Name, userNewOwner.Name);

    }

}


