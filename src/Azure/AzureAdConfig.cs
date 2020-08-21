using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Config For Azure AD
/// </summary>
internal partial class AzureAdConfig
{

    //ID/Password to sign into the Online Site
    public readonly string AzureAdTenantId;
    public readonly string AzureAdClientId;
    public readonly string AzureAdClientSecret;
    
    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="filePathConfig"></param>
    public AzureAdConfig(string filePathConfig)
    {

        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfigTargetSite = new System.Xml.XmlDocument();
        xmlConfigTargetSite.Load(filePathConfig);

        var xNodeAzureAd = xmlConfigTargetSite.SelectSingleNode("//Configuration/AzureAdLogin");
        this.AzureAdTenantId = xNodeAzureAd.Attributes["tenantId"].Value;
        this.AzureAdClientId = xNodeAzureAd.Attributes["clientId"].Value;
        this.AzureAdClientSecret = xNodeAzureAd.Attributes["secret"].Value;
    }
}