//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
internal static class IwsDiagnostics
{
//   private static bool _debugConsoleInitialized = false;
//    private static object _debugColsoleThreadLock = new object();

    /// <summary>
    /// This will serve as the debug-assert mechanism for this app
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="text"></param>
    public static void Assert(bool condition, string text)
    {
        if (condition) return;

        //System assert...
        System.Diagnostics.Debug.Assert(false, text);

        //UNDONE: (1) Write to debug file. (2) Add mechanism to return assert failures to client
        string assertFailure = text;

        try
        {
            AppLogging.LogAssertData(text);
        }
        catch(Exception e)
        {
            string eMessage = e.Message;
        }
    }

    /*
    /// <summary>
    /// Records a client error
    /// </summary>
    /// <param name="userAccount"></param>
    /// <param name="errorText"></param>
    /// <param name="clientContext"></param>
    internal static void RecordClientError_LoggedIn(UserAccount userAccount, string errorText, string clientContext)
    {
        RecordClientError(userAccount.Email + " (" + userAccount.UserId.ToString() + ")", errorText, clientContext);
    }


    /// <summary>
    /// Record a client error for an unauthenticated client
    /// </summary>
    /// <param name="errorText"></param>
    /// <param name="clientContext"></param>
    internal static void RecordClientError_NotLoggedIn(string errorText, string clientContext)
    {
        //If we are not recording unauthenticated errors, then exit
        if(!AppSettings.DiagnosticsWriteUnauthenticatedClientReportedErrorsToFile)
        {
            return;
        }

        RecordClientError("*UNAUTHENTICATED CLIENT*", errorText, clientContext);
    }
    */

    /// <summary>
    /// Trims text to the maxLength
    /// </summary>
    /// <param name="textIn"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    private static string TrimTextToLength(string textIn, int maxLength)
    {
        if ((string.IsNullOrEmpty(textIn)) || (textIn.Length < maxLength))
        {
            return textIn;
        }

        return textIn.Substring(0, maxLength);
    }

    /*
   /// <summary>
   /// Ensures we are properly setup for writing debug output to the console
   /// </summary>
   private static void EnsureDebugConsoleInitialized()
   {
       //If we are already setup, then exit
       if (_debugConsoleInitialized) return;

       //Ensure we only enter this once
       lock(_debugColsoleThreadLock)
       {
           if (_debugConsoleInitialized) return;

           //See if the config setting indicates that we should use the console out
           var debugConsole = System.Configuration.ConfigurationManager.AppSettings["iwsUseDebugOutput"];
           if ((debugConsole!= null) && (debugConsole.ToString().Trim().ToLower() == "console"))
           {
               //Register a listener to pipe debug-write output to the console
               Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
           }

           _debugConsoleInitialized = true; //Success
       }
   }
*/
    /// <summary>
    /// Writes diagnostic data
    /// </summary>
    /// <param name="text"></param>
    public static void WriteLine(string text)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#iws#####################################################");
        sb.Append("iws - writeline: ");
        sb.AppendLine(DateTime.UtcNow.ToString());
        sb.AppendLine(text);

        var writeText = sb.ToString();
        Debug.WriteLine(writeText);
        //Log it whereever else we want to output it
        AppLogging.LogDebugOutput(writeText);
    }


    /// <summary>
    /// Ranks security concerns
    /// </summary>
    public enum SecurityConcernLevel
    {
        Normal, //Represents a bogus client request
        High   //Represents a likely server-side internal error
    }

    /// <summary>
    /// Should be called to log any potential security concerns
    /// </summary>
    /// <param name="text"></param>
    public static void WriteLineSecurityConcern(string text, SecurityConcernLevel concernLevel = SecurityConcernLevel.High)
    {
        WriteLine("Security: " + text);
        AppLogging.LogSecurityConcern(text, concernLevel);
    }


    /// <summary>
    /// Log each controller called
    /// </summary>
    /// <param name="controllerName"></param>
    /// <param name="parmeters"></param>
    internal static void WriteLineControllerEntry(string controllerName, object[] parmeters)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Controller: ");
        sb.Append(controllerName);
        sb.Append("(");

        if(parmeters != null)
        { 
            int idxParam = 0;
            //Append each parameter to the log
            foreach(var p in parmeters)
            {
                if (idxParam > 0) sb.Append(", ");

                if (p != null)
                { 
                    sb.Append(p.ToString());
                }
                else
                {
                    sb.Append("null");
                }

                idxParam++;
            }
        }

        sb.Append(")");

        WriteLine(sb.ToString());
    }
}
