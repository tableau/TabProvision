using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/// <summary>
/// Gives parsed values for the command line arguments
/// </summary>
partial class CommandLineParser
{

    public const string Parameter_Command               = "-command";             //Specifies the top level command to the REST applicaiton
    public const string Parameter_ExitWhenDone          = "-exitWhenDone";        //When running as command line, if 'true' we will exit when the work is done
                                                                                  //    public const string Parameter_TargetSitePath        = "-targetSite";          //Target site config
    public const string Parameter_PathSecrets           = "-pathSecrets";          //path to secrets file
    public const string Parameter_PathProvisionPlan     = "-pathProvisionPlan";   //path to the provisioning plan we want to follow
    public const string Parameter_PathOutput            = "-pathOutput";          //path to output files we generate

    public const string Command_ProvisionFromAzure        = "provisionSiteFromAzure";  //Provision an Online site using data in Azure AD
    public const string Command_ProvisionFromFileManifest = "provisionSiteFromFile";   //Provision an Online site using an explicit file manifest

    /// <summary>
    /// Parse the command line value to a true/false
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    internal bool GetParameterValue_Boolean(string parameter, bool defaultValue)
    {
        string paramValueText  = GetParameterValue(parameter);
        if(string.IsNullOrWhiteSpace(paramValueText))
        {
            return defaultValue;
        }

        return System.Convert.ToBoolean(paramValueText);
    }

    /*    public const string Parameter_FromSiteUrl           = "-fromSiteUrl";         //URL to site we are accessing
   public const string Parameter_FromUserId            = "-fromSiteUserId";      //User ID we are downloading content from
   public const string Parameter_FromUserPassword      = "-fromSiteUserPassword";//Password for user id 
   public const string Parameter_FromSiteIsSiteAdmin   = "-fromSiteIsSiteAdmin";   //Is the user id a Site Admin account?
   public const string Parameter_FromSiteIsSystemAdmin = "-fromSiteIsSystemAdmin"; //Is the user id a System Admin account?
   public const string Parameter_ToSiteUrl             = "-toSiteUrl";           //URL to site we are accessing
   public const string Parameter_ToUserId              = "-toSiteUserId";        //User ID we are downloading content from
   public const string Parameter_ToUserPassword        = "-toSiteUserPassword";  //Password for user id 
   public const string Parameter_ToSiteIsSystemAdmin   = "-toSiteIsSystemAdmin"; //Is the user id a System Admin account?
   public const string Parameter_ToSiteIsSiteAdmin     = "-toSiteIsSiteAdmin";   //Is the user id a Site Admin account?
   public const string Parameter_ExportSingleProject   = "-exportSingleProject"; //If specified, only a single projects content will be exported
   public const string Parameter_ExportOnlyWithTag     = "-exportOnlyTagged";    //If specified, only content with the specified tag will be exported
   public const string Parameter_RemoveTagAfterExport  = "-exportOnlyTaggedRemoveTag"; //If specified, we will remove the tag from any exported content 
   public const string Parameter_DBCredentialsFile     = "-dbCredentialsFile";   //If specified, this points to the file we should get upload DB credentials from
   public const string Parameter_LogFile               = "-logFile";             //File for log output
   public const string Parameter_LogVerbose            = "-logVerbose";          //Verbose logging level
   public const string Parameter_BackgroundKeepAlive   = "-backgroundKeepAlive"; //Send periodic background requests to ensure the server session is kept alive
   public const string Parameter_GenerateInventoryTwb  = "-generateInventoryTwb";//Create a Tableau Workbook with inventory data
   public const string Parameter_ErrorsFile            = "-errorFile";           //File for error output
   public const string Parameter_ManualStepsFile       = "-manualStepsFile";     //File for recording manual steps for tasks that could not be automatically completed
   public const string Parameter_InventoryOutputFile   = "-inventoryOutputFile"; //Where the inventory output goes
   public const string Parameter_ExportDirectory       = "-exportDirectory";     //Where the site gets exported to
   public const string Parameter_ImportDirectory       = "-importDirectory";     //Where the site gets imported from
   public const string Parameter_ImportAssignContentOwnership  = "-remapContentOwnership"; //On site import, look for content metadata files that tell us what owner to assign the content to
   public const string Parameter_RemapDataserverReferences     = "-remapDataserverReferences"; //On site import, workbook XML should be examined and have data server references updated to point to the target server/site
   public const string Parameter_GenerateInfoFilesForDownloads = "-downloadInfoFiles"; //Downloaded Workbooks/Datasources will get companion XML files with additional metadata that can be used during uploads (e.g. show tabs in workbooks)
*/
    /*
        //Get an inventory of the running server
        public const string ParameterValue_Command_Inventory = "siteInventory";
        public const string ParameterValue_Command_Export    = "siteExport";
        public const string ParameterValue_Command_Import    = "siteImport";
    */
    //public const string ParameterValue_True              = "true";
    //public const string ParameterValue_False             = "false";

    //Standard dummy text we want to use for obscured passwords
  //  public const string DummyText_Password = "*****";


    /// <summary>
    /// TRUE if we believe there is enough information in the command line to proceed with running the headless task
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public static bool HasUseableCommandLine(CommandLineParser commandLine)
    {
        //If the command line does not contain a "-command" argument, then we know there's nothing we can do
        if(string.IsNullOrWhiteSpace(commandLine.GetParameterValue(Parameter_Command)))
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// Simple conversion bool to text
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    private static string helper_BoolToText(bool b)
    {
        if(b) return "true";
        return "false";
    }

    /// <summary>
    /// Appends a paramter and value
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="sb"></param>
    /// <param name="param"></param>
    /// <param name="value"></param>
    private static void helper_AppendParameter(List<string> arguments, StringBuilder sb, string param, string value)
    {
        //By default use the same value for the the arguments list and the string builder
        helper_AppendParameter(arguments, sb, param, value, value);
    }
    /// <summary>
    /// Appends a paramter and value
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="param"></param>
    /// <param name="value"></param>
    private static void helper_AppendParameter(List<string> arguments, StringBuilder sb, string param, string value, string valueForStringBuilder)
    {
        //Cannonicalize: Parameters all start with "-"
        if (param[0] != '-')
        {
            param = "-" + param;
        }

        value = value.Trim();
        //Add a space if there is already content in the command line
        if(sb.Length > 0)
        {
            sb.Append(" ");
        }

        sb.Append(param);

        sb.Append(" ");

        //If the value has any spaces, it needs quotes
        bool valueNeedsQuote = value.Contains(' ');
        if(valueNeedsQuote)
        {
            sb.Append("\"");  //Start qoute
        }

        sb.Append(valueForStringBuilder);

        if (valueNeedsQuote) 
        {
            sb.Append("\"");  //End qoute
        }

        //Append the arugments to the list
        arguments.Add(param);
        arguments.Add(value);
    }
}
