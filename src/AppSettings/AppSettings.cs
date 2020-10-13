//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using Microsoft.Win32;

/// <summary>
/// Global settings for the application
/// </summary>
internal static class AppSettings
{
    private static CommandLineParser s_CommandLineParsed;
    const string Registry_AppName = "TabProvision";
    const string Registry_AppKey = "HKEY_CURRENT_USER\\Software\\" + Registry_AppName;
    const string Registry_Preference_DefaultSecretsPath = "DefaultSecretsPath";
    const string Registry_Preference_DefaultAzureAdConfigPath = "DefaultAzureAdProvisioningConfigPath";
    const string Registry_Preference_DefaultFileProvisioningPath = "DefaultFileProvisioningConfigPath";
    const string Registry_Preference_DefaultTargetPathTableauSiteSourcedManifest = "TargetPathTableauSiteSourcedManifest";

    /// <summary>
    /// Load the user's preferred path to their secrets file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathSecretsConfig()
    {
        return LoadRegistryString(Registry_Preference_DefaultSecretsPath);
    }

    /// <summary>
    /// Save the user's preferred path to their secrets file
    /// </summary>
    /// <param name="text"></param>
    internal static void SavePreference_PathSecretsConfig(string text)
    {
        SaveRegistryValue(Registry_Preference_DefaultSecretsPath, text);
    }

    /// <summary>
    /// Load the user's preferred path to the AzureAD provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathAzureAdProvisioningConfig()
    {
        return LoadRegistryString(Registry_Preference_DefaultAzureAdConfigPath);
    }


    /// <summary>
    /// Save the user's preferred path to the AzureAD provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static void SavePreference_PathAzureAdProvisioningConfig(string text)
    {
        SaveRegistryValue(Registry_Preference_DefaultAzureAdConfigPath, text);
    }

    /// <summary>
    /// Load the user's preferred path to the File provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathFileProvisioningConfig()
    {
        return LoadRegistryString(Registry_Preference_DefaultFileProvisioningPath);
    }


    /// <summary>
    /// Load the perferred location to write Tableau sources manifed
    /// </summary>
    /// <returns></returns>
    internal static string LoadPreference_TargetPathTableauSiteSourcedManifest()
    {
        return LoadRegistryString(Registry_Preference_DefaultTargetPathTableauSiteSourcedManifest);
    }

    /// <summary>
    /// Save the user's preferred path to the File provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static void SavePreference_PathFileProvisioningConfig(string text)
    {
        SaveRegistryValue(Registry_Preference_DefaultFileProvisioningPath, text);
    }

    /// <summary>
    /// User's perferred location for the target of Tableau Site generated manifests
    /// </summary>
    /// <param name="text"></param>
    internal static void SavePreference_TargetPathTableauSiteSourcedManifest(string text)
    {
        SaveRegistryValue(Registry_Preference_DefaultTargetPathTableauSiteSourcedManifest, text);
    }

    /// <summary>
    /// Saves a value to the Windows registry (save a user preference)
    /// </summary>
    /// <param name="preferenceName"></param>
    /// <param name="value"></param>
    private static void SaveRegistryValue(string preferenceName, string value)
    {
        //Store the value
        Registry.SetValue(Registry_AppKey, preferenceName, value);
    }

    /// <summary>
    /// Saves a string from the Windows registry (load a user preference)
    /// </summary>
    /// <param name="preferenceName"></param>
    /// <param name="value"></param>
    private static string LoadRegistryString(string preferenceName)
    {
        try
        {
        //Get the value as a string
        return (string) Registry.GetValue(Registry_AppKey, preferenceName, "");
        }
        catch (Exception ex)
        {
            IwsDiagnostics.Assert(false, "819-1040: Error loading registry value, " + preferenceName + ", " + ex.Message);
            return "";
        }
    }

    public static void SetCommandLine(CommandLineParser commandLineParsed)
    {
        s_CommandLineParsed = commandLineParsed;
    }

    public static string CommandLine_Command
    {
        get
        {
            if (s_CommandLineParsed == null) return null;
            return s_CommandLineParsed.GetParameterValue(CommandLineParser.Parameter_Command);
        }
    }

    /// <summary>
    /// Path to the XML file that holds the sign in secrets
    /// </summary>
    public static string CommandLine_PathSecrets
    {
        get
        {
            if (s_CommandLineParsed == null) return null;
            return s_CommandLineParsed.GetParameterValue(CommandLineParser.Parameter_PathSecrets);
        }
    }
    
    /// <summary>
    /// Path to the XML file that holds the provisioning plan
    /// </summary>
    public static string CommandLine_PathProvisionPlan
    {
        get
        {
            if (s_CommandLineParsed == null) return null;
            return s_CommandLineParsed.GetParameterValue(CommandLineParser.Parameter_PathProvisionPlan);
        }
    }

    /// <summary>
    /// Path to directory we want to output results to
    /// </summary>
    public static string CommandLine_PathOutput
    {
        get
        {
            if (s_CommandLineParsed == null) return null;
            return s_CommandLineParsed.GetParameterValue(CommandLineParser.Parameter_PathOutput);
        }
    }

    /// <summary>
    /// If TRUE, the app should exit after it has run the command
    /// </summary>
    public static bool CommandLine_ExitWhenDone
    {
        get
        {
            //No command line?  Don't exit when done
            if (s_CommandLineParsed == null) return false;

            //Command line: Default to exit when done
            return s_CommandLineParsed.GetParameterValue_Boolean(
                CommandLineParser.Parameter_ExitWhenDone, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool CommandLine_IgnoreAllUsersGroup
    {
        get
        {
            //No command line?  
            if (s_CommandLineParsed == null) return true;

            //Command line: Default to exit when done
            return s_CommandLineParsed.GetParameterValue_Boolean(
                CommandLineParser.Parameter_IgnoreAllUsersGroup, true);
        }
    }

    /// <summary>
    /// Looks up an attribute by name and returns true/false
    /// </summary>
    /// <param name="attributeName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static bool GetAppSettingIntegerBoolean(string attributeName, bool defaultValue)
    {
        IwsDiagnostics.Assert(!string.IsNullOrWhiteSpace(attributeName), "160615-1959, missing attribute name");

        var textValue = ConfigurationManager.AppSettings[attributeName];

        if (string.IsNullOrWhiteSpace(textValue))
        {
            return defaultValue;
        }

        textValue = textValue.Trim().ToLower();
        if (textValue == "true") return true;
        if (textValue == "false") return false;

        //Abort
        IwsDiagnostics.Assert(
            false,
            "160615-1958, attribute value not true/false, " + attributeName + "/" + textValue);
        throw new ArgumentException("160615-1958, attribute value not true/false, " + attributeName + "/" + textValue);
    }

    /// <summary>
    /// Safe way to get a setting with a default value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    private static string GetAppSettingString(string settingName, string defaultValue = "")
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "151210-0601, missing setting key");
            throw new ArgumentException("151210-0601, missing setting key");
        }

        
        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return value;
    }

    /// <summary>
    /// Returns an integer value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static int GetAppSettingInteger(string settingName, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "151210-0602, missing setting key");
            throw new ArgumentException("1210-0602, missing setting key");
        }

        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return System.Convert.ToInt32(value);
    }


    /// <summary>
    /// Returns an double value
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static double GetAppSettingDouble(string settingName, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(settingName))
        {
            IwsDiagnostics.Assert(false, "925-902, missing setting key");
            throw new ArgumentException("925-902, missing setting key");
        }

        string value = ConfigurationManager.AppSettings[settingName];
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return System.Convert.ToDouble(value);
    }



    /// <summary>
    /// Path for storing temp data about a site (e.g. downloaded thumbnails)
    /// </summary>
    /// <param name="siteSignIn"></param>
    /// <returns></returns>
    public static string LocalFileSystemPath_TempSpace_Site(TableauServerSignIn siteSignIn)
    {
        if((siteSignIn == null) || (string.IsNullOrEmpty(siteSignIn.SiteId)))
        {
            throw new Exception("1025-1053: No signed in site info");
        }

        var siteId = siteSignIn.SiteId;
        if (!RegExHelper.IsValidIdTableauContentId(siteId))
        {
            throw new Exception("1029-815, invalid site id");
        }

        var fullPath = System.IO.Path.Combine(AppSettings.LocalFileSystemPath_TempSpace, @"Sites");
        fullPath = System.IO.Path.Combine(fullPath, "site_" + siteId);
        //Create the directory if we need to
        FileIOHelper.CreatePathIfNeeded(fullPath);

        return fullPath;

    }

    /// <summary>
    /// Returns the local file system path for temporary working files
    /// </summary>
    public static string LocalFileSystemPath_TempSpace
    {
        get
        {
            var fullPath =
                System.IO.Path.Combine(AppSettings.LocalFileSystemPath, @"Temp");

            //Create the directory if we need to
            FileIOHelper.CreatePathIfNeeded(fullPath);

            return fullPath;
        }
    }

    /// <summary>
    /// TRUE: We want to write assert contents into files
    /// </summary>
    public static bool DiagnosticsWriteDebugOutputToFile
    {
        get
        {
            return GetAppSettingIntegerBoolean(
                "iwsDiagnosticsWriteDebugOutputToFile",
                false); //Default to not logging user actions
        }
    }


   
    /// <summary>
    /// Returns the local file system path for the application
    /// </summary>
    public static string LocalFileSystemPath
    {
        get
        {
            return AppDomain.CurrentDomain.GetData("APPBASE").ToString();
        }
    }

    /// <summary>
    /// Returns the local file system path for photo storage
    /// </summary>
    public static string LocalFileSystemPath_Diagnostics
    {
        get
        {
            var fullPathToPhotoDirectory =
                System.IO.Path.Combine(AppSettings.LocalFileSystemPath, @"App_Data\iwsPrivateContent\Diagnostics");

            return fullPathToPhotoDirectory;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static bool DiagnosticsWriteAssertsToFile
    {
        get
        {
            return false;
        }
    }

}