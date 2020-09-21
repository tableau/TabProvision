using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


internal partial class ProvisioningGroup
{
    /// <summary>
    /// Instructions on how to apply Grant License Modes for a group
    /// </summary>
    public enum GrantLicenseMode
    {
        Ignore, //We don't care what the Grant License Mode of the group is
        None,   //The Group should have NO Grant License Mode
        OnLogin //The Group should have a grant license mode of OnLogin
    }

    //Allowable attribute valie
    public const string GrantLicenseMode_Ignore = "ignore";
    public const string GrantLicenseMode_None = "none";
    public const string GrantLicenseMode_OnLogin = "onLogin";

    /// <summary>
    /// Generate XML attribute text
    /// </summary>
    /// <param name="grantLicenseMode"></param>
    /// <returns></returns>
    public static string GenerateGrantLicenseModeAttributeText(GrantLicenseMode grantLicenseMode)
    {
        switch(grantLicenseMode)
        {
            case GrantLicenseMode.Ignore:
                return GrantLicenseMode_Ignore;

            case GrantLicenseMode.None:
                return GrantLicenseMode_None;

            case GrantLicenseMode.OnLogin:
                return GrantLicenseMode_OnLogin;

            default:
                IwsDiagnostics.Assert(false, "920-618: Internal error. Unknown Grant License Mode");
                throw new Exception("920-618: Internal error. Unknown Grant License Mode");
        }
    }

    /// <summary>
    /// Parse it (from an XML attribute)
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static GrantLicenseMode ParseGrantLicenseMode(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            return GrantLicenseMode.Ignore; 
        }

        if(text == GrantLicenseMode_Ignore) { return GrantLicenseMode.Ignore; }
        if (text == GrantLicenseMode_None) { return GrantLicenseMode.None; }
        if (text == GrantLicenseMode_OnLogin) { return GrantLicenseMode.OnLogin; }

        throw new Exception("920-942: Unknown Grant License Mode: " + text);
    }
}