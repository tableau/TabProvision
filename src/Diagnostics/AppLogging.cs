//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Data.Sql;
using System.Data.SqlClient;

internal static class AppLogging 
{
    /*
    /// <summary>
    /// Logs a journal entry with a series of parameters.
    /// 
    /// The goal here is simple debugging/auditing. Don't yet have a need for a formal play-back mechanism
    /// to flawlessly recreate the actions from the journal
    /// </summary>
    /// <param name="actionName"></param>
    /// <param name="actionData"></param>
    public static void LogJournalData(DBConnectionManager dbConnections, UserAccount user, string actionName, string[] actionData)
    {
        //Just pack the parameters into a simple string and log it to the journal
        var sb = new StringBuilder();
        bool isFirst = true;
        foreach (var str in actionData)
        {
            if (!isFirst)
            {
                sb.Append(" || "); //Simple splitter
            }

            isFirst = false;
            sb.Append(str); //Add the parameter
        }

        LogJournalData(dbConnections, user, actionName, sb.ToString());

    }
    */
    /*
    /// <summary>
    /// Called to log a failed assert to the database
    /// </summary>
    /// <param name="assertDetails"></param>
    public static void LogClientReportedError(string assertDetails)
    {
        //See if we are supposed to log to a file
        if (AppSettings.DiagnosticsWriteClientReportedErrorsToFile)
        {
            try
            {
                LogClientReportedErrorToFile(assertDetails);
            }
            catch
            {
                //Something happened trying to write out the file contents... march on.
            }
        }
    }
    */
        /// <summary>
        /// Called to log a failed assert to the database
        /// </summary>
        /// <param name="assertDetails"></param>
        public static void LogAssertData(string assertDetails)
    { 
        //See if we are supposed to log to a file
        if(AppSettings.DiagnosticsWriteAssertsToFile)
        {
            try
            {
                LogAssertDataToFile(assertDetails);
            }
            catch
            {
                //Something happened trying to write out the file contents... march on.
            }
        }
        /*
        using (var dbConnectManager = new DBConnectionManager())
        {


            //FUTURE: Long term, we want to write this somehwere other than our journaling table
            //LogJournalData(dbConnectManager, LogInManager.GetLoggedInUser(dbConnectManager), "ASSERT-FAILED", assertDetails);
        }
        */
    }

    /// <summary>
    /// Log security concerns to a file
    /// </summary>
    /// <param name="details"></param>
    public static void LogSecurityConcern(string details, IwsDiagnostics.SecurityConcernLevel concernLevel)
    {
        if (AppSettings.DiagnosticsWriteSecurityConcernsToFile)
        {
            try
            {
                //Put it in a file
                LogSecurityConcernToFile(details, concernLevel);
            }
            catch
            {
                //Something happened trying to write out the file contents... march on.
            }
        }
    }

    /// <summary>
    /// Called to log a debug output to a file
    /// </summary>
    /// <param name="assertDetails"></param>
    public static void LogDebugOutput(string textout)
    {
        //See if we are supposed to log to a file
        if (AppSettings.DiagnosticsWriteDebugOutputToFile)
        {
            try
            {
                WriteDiagnisticTextToFile("debugOutput", textout);
            }
            catch
            {
                //Something happened trying to write out the file contents... march on.
            }
        }
    }

    /// <summary>
    /// Logs assert data into a date names file on the local file system
    /// </summary>
    /// <param name="assertDetails"></param>
    private static void LogAssertDataToFile(string assertDetails)
    {
        var dateTimeNow = DateTime.UtcNow;
        var sb = new StringBuilder();

        sb.Append("Assert timestamp: ");
        sb.AppendLine(dateTimeNow.ToString());

        sb.Append("Assert text: ");
        sb.AppendLine(assertDetails);
        sb.AppendLine("--------------------------------------------------------");
        sb.AppendLine();

        //Write the data to the output file
        WriteDiagnisticTextToFile("assert", sb.ToString());
    }

        /// <summary>
        /// Logs assert data into a date names file on the local file system
        /// </summary>
        /// <param name="assertDetails"></param>
        private static void LogClientReportedErrorToFile(string assertDetails)
        {
            var dateTimeNow = DateTime.UtcNow;
            var sb = new StringBuilder();

            sb.Append("Client error timestamp: ");
            sb.AppendLine(dateTimeNow.ToString());

            sb.Append("Error text: ");
            sb.AppendLine(assertDetails);
            sb.AppendLine("--------------------------------------------------------");
            sb.AppendLine();

            //Write the data to the output file
            WriteDiagnisticTextToFile("clientError", sb.ToString());
        }

        /// <summary>
        /// Logs security concern data into a date names file on the local file system
        /// </summary>
        /// <param name="details"></param>
        private static void LogSecurityConcernToFile(string details, IwsDiagnostics.SecurityConcernLevel concernLevel)
    {
        var dateTimeNow = DateTime.UtcNow;
        var sb = new StringBuilder();

        //If the concern level is high, note it
        if(concernLevel == IwsDiagnostics.SecurityConcernLevel.High)
        {
            sb.AppendLine("Security concern level: HIGH-CONCERN");
        }

        sb.Append("Security concern timestamp: ");
        sb.AppendLine(dateTimeNow.ToString());

        sb.Append("Security concern text: ");
        sb.AppendLine(details);
        sb.AppendLine("--------------------------------------------------------");
        sb.AppendLine();

        //Write the data to the output file
        WriteDiagnisticTextToFile("securityconcern", sb.ToString());
    }

    /// <summary>
    /// Writes out a chunk of text to a diagnostic file
    /// </summary>
    /// <param name="filenameBase">(e.g. 'assert', 'debug').  Must be only A-Z characters</param>
    /// <param name="writeText"></param>
    private static void WriteDiagnisticTextToFile(string filenameBase, string writeText)
    {
        //SECURITY CHECK: Allow only simple alphabetic filename. 
        //We should NEVER hit this if we are called with normal file names
        if(!RegExHelper.IsAlphabeticText(filenameBase))
        {
            throw new Exception("67-1258: Unexpected debugging filename: " + filenameBase);
        }

        var dateTimeNow = DateTime.UtcNow;
        string filename = filenameBase + dateTimeNow.Year.ToString() + dateTimeNow.Month.ToString("00") + dateTimeNow.Day.ToString("00") + ".txt";

        //Gent the directory
        var directoryAsserts = AppSettings.LocalFileSystemPath_Diagnostics;
        FileIOHelper.CreatePathIfNeeded(directoryAsserts);

        var filenameWithPath = Path.Combine(directoryAsserts, filename);
        //Write the assert contents into the file
        File.AppendAllText(filenameWithPath, writeText);

    }

    /*
    /// <summary>
    /// Log a debugging/auditing action
    ///
    /// The goal here is simple debugging/auditing. Don't yet have a need for a formal play-back mechanism
    /// to flawlessly recreate the actions from the journal
    /// </summary>
    /// <param name="logMe"></param>
    public static void LogJournalData(DBConnectionManager dbConnections, UserAccount user, string actionName, string actionData)
    {
        using (var command = new SqlCommand(
                        "Insert INTO iwsAppJournal ([date], [user-id], [action-name], [action-data]) " +
                                           "VALUES (@date,  @userid,   @actionname,   @actiondata)", 
                        dbConnections.WriteableOpenedConnection))
        {
            var sqlParams = command.Parameters;
            sqlParams.AddWithValue("@date", DateTime.UtcNow);
            sqlParams.AddWithValue("@userid", user.Email);
            sqlParams.AddWithValue("@actionname", actionName);
            sqlParams.AddWithValue("@actiondata", actionData);

            var rows = command.ExecuteNonQuery();

        }
    }
    */
}
