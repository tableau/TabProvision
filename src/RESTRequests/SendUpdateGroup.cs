using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Windows.Forms;
using Microsoft.Graph;
using System.Runtime.CompilerServices;

/// <summary>
/// Attempts to update a Group on the Tableau Site
/// 
/// REST API reference: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#update_group
/// </summary>
class SendUpdateGroup : TableauServerSignedInRequestBase
{
    /*
        /// <summary>
        /// URL manager
        /// </summary>
        private readonly TableauServerUrls _onlineUrls;
    */
    public readonly string GroupId;
    public readonly string UpdatedGroupName;

    /// <summary>
    /// If this value is FALSE then the Grant license values are not applied
    /// </summary>
    public readonly bool PerformUpdateGrantLicense;
    public readonly string UpdatedGrantLicenseMode;
    public readonly string UpdatedGrantLicenseSiteRole;

    /// <summary>
    /// Constructor
    /// 
    /// NOTE: [2020-09-20] Currently this class does NOT work with Active Directory (on premises server) synchronized groups
    /// </summary>
    /// <param name="login"></param>
    /// <param name="groupId"></param>
    /// <param name="updatedGroupName"></param>
    /// <param name="performGrantLicenseUpdate">FALSE: Ignore the Grant License data</param>
    /// <param name="grantLicenseMode">"" or "onLogin"</param>
    /// <param name="grantLicenseSiteRole">"Viewer", "Explorer", "ExplorerCanPublish", "Creator", site admin roles...</param>
    public SendUpdateGroup(
        TableauServerSignIn login,
        string groupId,
        string updatedGroupName,
        bool performGrantLicenseUpdate = false,
        string grantLicenseMode = null,
        string grantLicenseSiteRole = null)
        : base(login)
    {
        this.GroupId = groupId;
        this.UpdatedGroupName = updatedGroupName;

        this.PerformUpdateGrantLicense = performGrantLicenseUpdate;
        this.UpdatedGrantLicenseMode = grantLicenseMode;
        this.UpdatedGrantLicenseSiteRole = grantLicenseSiteRole;

    }

    /// <summary>
    /// Change the Group
    /// </summary>
    /// <param name="serverName"></param>
    public bool ExecuteRequest()
    {
        try
        {
            var updatedSuccess = UpdateGroup();
            this.StatusLog.AddStatus("Site group updated:" + this.GroupId + "/" + this.UpdatedGroupName);
            return updatedSuccess;
        }
        catch (Exception exError)
        {
            this.StatusLog.AddError("Error attempting to update group: " +this.GroupId + "/" + this.UpdatedGroupName + "', " + exError.Message);
            return false;
        }
    }


    /// <summary>
    /// Update the group
    /// </summary>
    /// <returns>TRUE: Success.  FALSE: failed/error</returns>
    private bool UpdateGroup()
    {
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(this.GroupId), "920-1053: missing group id");
        AppDiagnostics.Assert(!string.IsNullOrWhiteSpace(this.UpdatedGroupName), "920-1054: missing role");

        //[2020-09-20] NOTE: Currently this function DOES NOT work for Active Directory syncronized groups (on premises Tableau Serer)

        //ref: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#update_group
        var sb = new StringBuilder();
        var xmlWriter = XmlWriter.Create(sb, XmlHelper.XmlSettingsForWebRequests);
        xmlWriter.WriteStartElement("tsRequest");
        xmlWriter.WriteStartElement("group");
            xmlWriter.WriteAttributeString("name", this.UpdatedGroupName);

            //If we are updating the grant license parts, write these here
            //[2020-09-20] For (on premises Server) Active Directory sync, this would need to be different XML (inside an "import" node)
            if(this.PerformUpdateGrantLicense)
            {
                xmlWriter.WriteAttributeString("grantLicenseMode", this.UpdatedGrantLicenseMode);

                //If the Grant license mode is blank, set the license role to be UNLICENSED
                //This is required to remove the licensing mode: https://help.tableau.com/current/api/rest_api/en-us/REST/rest_api_ref.htm#update_group
                string updateLicenseMode = this.UpdatedGrantLicenseSiteRole;
                if (string.IsNullOrEmpty(this.UpdatedGrantLicenseMode))
                    {
                        updateLicenseMode = "UNLICENSED";
                    }
                xmlWriter.WriteAttributeString("minimumSiteRole", updateLicenseMode);
            }

        xmlWriter.WriteEndElement();//</group>
        xmlWriter.WriteEndElement(); // </tsRequest>
        xmlWriter.Close();

        var xmlText = sb.ToString(); //Get the XML text out

        //Create a web request 
        var urlUpdateGroup = this._onlineSession.ServerUrls.Url_UpdateSiteGroup(_onlineSession, this.GroupId);
        var webRequest = this.CreateLoggedInWebRequest(urlUpdateGroup, "PUT");
        TableauServerRequestBase.SendPutContents(webRequest, xmlText);
        
        //Get the response
        var response = GetWebReponseLogErrors(webRequest, "update group (change name/grant-license)");
        using (response)
        {
            var xmlDoc = GetWebResponseAsXml(response);
            
            //Get all the group nodes
            var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
            var xNodeUpdatedGroup = xmlDoc.SelectSingleNode("//iwsOnline:group", nsManager);

            try
            {
                //Sanity check on the expected results
                var updatesGroupId = xNodeUpdatedGroup.Attributes["id"].Value;
                var returnedMinSiteRole = XmlHelper.SafeParseXmlAttribute(xNodeUpdatedGroup, "minimumSiteRole", "");

                if (updatesGroupId != this.GroupId)
                {
                    IwsDiagnostics.Assert(false, "920-1102: Error. Updated groups returned mismatching group id: " + this.GroupId + "/" + updatesGroupId);
                    StatusLog.AddError("920-1102: Error. Updated groups returned mismatching group id: " + this.GroupId + "/" + updatesGroupId);
                    return false;
                }

                //See if the returned role matches
                if(this.PerformUpdateGrantLicense)
                {
                    if(!CompareGrantLicenseRoles(returnedMinSiteRole, this.UpdatedGrantLicenseSiteRole))
                    {
                        string errorText = "920-1206: Error. Updated Grant License role for group does not match expected role: "
                            + NullSafeText(returnedMinSiteRole) + "/" + NullSafeText(this.UpdatedGrantLicenseSiteRole);
                        StatusLog.AddError(errorText);
                        return false;
                    }
                }

            }
            catch (Exception parseXml)
            {
                StatusLog.AddError("920-1105: Update group, error parsing XML response " + parseXml.Message + "\r\n" + xmlDoc.InnerXml);
                return false;
            }

        }
        return true; //Success
    }

    /// <summary>
    /// Simple helper
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string NullSafeText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }
        return text;
    }

    /// <summary>
    /// Comapare to role update texts
    /// </summary>
    /// <param name="role1"></param>
    /// <param name="role2"></param>
    /// <returns></returns>
    private static bool CompareGrantLicenseRoles(string role1, string role2)
    {
        if(string.IsNullOrEmpty(role1))
        {
            return string.IsNullOrEmpty(role2);
        }

        if (string.IsNullOrEmpty(role2))
        {
            return string.IsNullOrEmpty(role1);
        }

        return role1 == role2;
    }

}
