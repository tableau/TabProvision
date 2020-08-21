using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;


/// <summary>
/// Config that we will pass through the Provision Site process
/// </summary>
internal partial class ProvisionConfigSiteAccess
{

    //ID/Password to sign into the Online Site
    public readonly string SiteClientId;
    public readonly string Secret;

    /// <summary>
    /// If TRUE, email will not get sent. Instead it will be sent to the console or to a local file
    /// </summary>
//    public readonly bool EmailDivertToConsole;


    //URL for the site we want to run the report on
    public readonly string SiteUrl;

    //Log in mechanism to Online site
    public readonly TableauServerSignIn.SignInMode SiteSignInMode; //= TableauServerSignIn.SignInMode.UserNameAndPassword;

    //Allowed ways to sign in
    const string SignInMode_NamePassword = "NameAndPassword";
    const string SignInMode_PersonalAccessToken = "PersonalAccessToken";

    /// <summary>
    /// CONSTRUCTOR
    /// </summary>
    /// <param name="pathBaseConfig"></param>
    /// <param name="targetSiteConfig"></param>
    public ProvisionConfigSiteAccess(string targetSiteConfig)
    {
        //==================================================================================
        //Load values from the TARGET SITE config file
        //==================================================================================
        var xmlConfigTargetSite = new System.Xml.XmlDocument();
        xmlConfigTargetSite.Load(targetSiteConfig);

        //Site URL
        this.SiteUrl = xmlConfigTargetSite.SelectSingleNode("//Configuration/SiteUrl").Attributes["value"].Value;

        var xNodeTableauSite = xmlConfigTargetSite.SelectSingleNode("//Configuration/TableauSiteLogin");

        this.SiteSignInMode = ParseSignInModeAttribute(xNodeTableauSite);
        this.SiteClientId = xNodeTableauSite.Attributes["clientId"].Value;
        this.Secret = xNodeTableauSite.Attributes["secret"].Value;

    }

    /// <summary>
    /// Parse the sign in mode
    /// </summary>
    /// <param name="xNodeTableauSite"></param>
    /// <returns></returns>
    private static TableauServerSignIn.SignInMode ParseSignInModeAttribute(XmlNode xNodeTableauSite)
    {
        string signInMode = xNodeTableauSite.Attributes["signInMode"].Value;
        switch (signInMode)
        {
            case SignInMode_NamePassword:
                return TableauServerSignIn.SignInMode.UserNameAndPassword;
            case SignInMode_PersonalAccessToken:
                return TableauServerSignIn.SignInMode.AuthToken;
            default:
                IwsDiagnostics.Assert(false, "819-1135: Unkown sign in mode, " + signInMode);
                throw new Exception("819-1135: Unkown sign in mode, " + signInMode);
        }
    }


    /// <summary>
    /// If the XPath query succeeds, return the value
    /// </summary>
    /// <param name="xmlDoc"></param>
    /// <param name="xpathQuery"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    private bool GetAtributeIfNodeExists_Boolean(XmlDocument xmlDoc, string xpathQuery, string attributeName, bool defaultValue)
    {
        var xmlNode = xmlDoc.SelectSingleNode(xpathQuery);
        //If the node does not exist, return null
        if (xmlNode == null)
        {
            return defaultValue;
        }

        return XmlHelper.SafeParseXmlAttribute_Boolean(xmlNode, attributeName, defaultValue);
    }


    /// <summary>
    /// If the XPath query succeeds, return the value
    /// </summary>
    /// <param name="xmlDoc"></param>
    /// <param name="xpathQuery"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    private string GetAtributeIfNodeExists(XmlDocument xmlDoc, string xpathQuery, string attributeName)
    {
        var xmlNode = xmlDoc.SelectSingleNode(xpathQuery);
        //If the node does not exist, return null
        if (xmlNode == null)
        {
            return null;
        }

        var attribute = xmlNode.Attributes[attributeName];
        if (attribute == null)
        {
            return null;
        }

        return attribute.Value;
    }
}