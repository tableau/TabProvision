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

}
