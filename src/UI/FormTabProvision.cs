using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.CompilerServices;

namespace OnlineContentDownloader
{
    public partial class FormTabProvision : Form, IShowLogs
    {



        public FormTabProvision()
        {
            InitializeComponent();
        }


  
        /// <summary>
        /// Called to exit the application
        /// </summary>
        private void ExitApplication()
        {
            this.Close();
        }


        /// <summary>
        /// Open a file in the Windows shell (e.g. open a textfile or csv file)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool AttemptToShellFile(string path)
        {
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch(Exception ex)
            {
                AppDiagnostics.Assert(false, "Failure to shell file: " + ex.Message);
                return false;
            }

            return true; //Success
        }

        /// <summary>
        /// Shows status text in the textboxes
        /// </summary>
        /// <param name="statusLog"></param>
        private void UpdateStatusText(TaskStatusLogs statusLog, bool forceUIRefresh = false)
        {
            textBoxStatus.Text = statusLog.StatusText;
            ScrollToEndOfTextbox(textBoxStatus);

            textBoxErrors.Text = statusLog.ErrorText;

            if(forceUIRefresh)
            {
                textBoxStatus.Refresh();
                textBoxErrors.Refresh();
            }
        }


        //Scroll to the end
        private static void ScrollToEndOfTextbox(TextBox textbox)
        {
            textbox.SelectionStart = textbox.Text.Length;
            textbox.ScrollToCaret();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            //If the user is closing the app, then save the preferences
            if(e.CloseReason == CloseReason.UserClosing)
            {
                AppPreferences_Save();
            }

            Application.Exit();
        }

        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load()
        {
            try
            {
                AppPreferences_Load_Inner();
            }
            catch (Exception ex)
            {
                IwsDiagnostics.Assert(false, "819-1041: Error loading app prefernces, " + ex.Message);
            }
        }

        /// <summary>
        /// Load app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Load_Inner()
        {
            txtPathToSecrets.Text = AppSettings.LoadPreference_PathSecretsConfig();
            txtPathToAzureAdProvisioningConfig.Text = AppSettings.LoadPreference_PathAzureAdProvisioningConfig();
            txtPathToFileProvisioningConfig.Text = AppSettings.LoadPreference_PathFileProvisioningConfig();

            //If any of these are blank then generate them
            txtPathToSecrets.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToSecrets.Text, "Templates_Secrets\\Example_Secrets.xml");
            txtPathToAzureAdProvisioningConfig.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToAzureAdProvisioningConfig.Text, "Templates\\ExampleSyncConfig_AzureAD.xml");
            txtPathToFileProvisioningConfig.Text = AppPreferences_GenerateDefaultIfBlank(txtPathToFileProvisioningConfig.Text, "Templates\\ExampleSyncConfig_FileSystem.xml");
        }


        /// <summary>
        /// If the proposed path is blank, then generate a path based on the applicaiton's path and the specified sub-path
        /// </summary>
        /// <param name="proposedPath"></param>
        /// <param name="subPath"></param>
        /// <returns></returns>
        private string AppPreferences_GenerateDefaultIfBlank(string proposedPath, string subPath)
        {
            if(!string.IsNullOrWhiteSpace(proposedPath))
            {
                return proposedPath;
            }

            var basePath = AppSettings.LocalFileSystemPath;
            return Path.Combine(basePath, subPath);
        }



        /// <summary>
        /// Store app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Save()
        {
            try
            {
                AppPreferences_Save_Inner();
            }
            catch(Exception ex)
            {
                IwsDiagnostics.Assert(false, "819-1014: Error storing app prefernces, " + ex.Message);
            }
        }

        /// <summary>
        /// Store app preferences that we want to auto-load next time
        /// </summary>
        private void AppPreferences_Save_Inner()
        {
            AppSettings.SavePreference_PathSecretsConfig(txtPathToSecrets.Text);
            AppSettings.SavePreference_PathAzureAdProvisioningConfig(txtPathToAzureAdProvisioningConfig.Text);
            AppSettings.SavePreference_PathFileProvisioningConfig(txtPathToFileProvisioningConfig.Text);
        }




        /// <summary>
        /// Called to run a command line task
        /// </summary>
        internal void RunStartupCommandLine()
        {
            var statusLogs = new TaskStatusLogs();

            try
            {
                RunStartupCommandLine_Inner(statusLogs);
            }
            catch(Exception ex)
            {
                IwsDiagnostics.Assert(false, "1101-445: Command line error: " + ex.Message);
            }

            //If we are supposed to exit, then do so...
            if (AppSettings.CommandLine_ExitWhenDone)
            {
                ExitApplication();
                return;
            }

            UpdateStatusText(statusLogs);
        }

        /// <summary>
        /// Called to run us in commandn line mode
        /// </summary>
        /// <param name="commandLine"></param>
        internal void RunStartupCommandLine_Inner(TaskStatusLogs statusLogs)
        {

            statusLogs.AddStatusHeader("Processing command line");
            string pathProvisionPlan = AppSettings.CommandLine_PathProvisionPlan;
            string pathSecrets = AppSettings.CommandLine_PathSecrets;
            string pathOutput = AppSettings.CommandLine_PathOutput;

            //If an output directory was not specified, then output into an "out" subdirectory in the directory where the provision plan is
            if(string.IsNullOrWhiteSpace(pathOutput))
            {
                pathOutput = Path.Combine(
                        Path.GetDirectoryName(pathProvisionPlan),
                        "out");
            }

            //====================================================================================
            //Based on the command specified, run the specified task
            //====================================================================================
            switch (AppSettings.CommandLine_Command)
            {

                case CommandLineParser.Command_ProvisionFromAzure:
                    //Update the paths in the UI so the user can see & re-run them if they want
                    txtPathToSecrets.Text = pathSecrets;
                    txtPathToAzureAdProvisioningConfig.Text = pathProvisionPlan;

                    //Run the work...
                    ProvisionFromAzureAd(statusLogs, pathSecrets, pathProvisionPlan, pathOutput);
                    break;

                case CommandLineParser.Command_ProvisionFromFileManifest:
                    //Update the paths in the UI so the user can see & re-run them if they want
                    txtPathToSecrets.Text = pathSecrets;
                    txtPathToFileProvisioningConfig.Text = pathProvisionPlan;

                    //Run the work...
                    ProvisionFromFileManifest(statusLogs, pathSecrets, pathProvisionPlan, pathOutput);
                    break;

                default:
                    statusLogs.AddError("1101-432: Unknown command: " + AppSettings.CommandLine_Command);
                    break;
            }

        }

        /// <summary>
        /// Command line path
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="secretsPath"></param>
        /// <param name="planPath"></param>
        /// <param name="outputPath"></param>
        private void GenerateProvisioningCommandLine(string commandName, string secretsPath, string planPath, string outputPath)
        {
            const string appName = "TabProvision.exe";
            string commandText =
                appName
                + " " + CommandLineParser.Parameter_Command           + " " + commandName
                + " " + CommandLineParser.Parameter_PathSecrets       + " \"" + secretsPath + "\""
                + " " + CommandLineParser.Parameter_PathProvisionPlan + " \"" + planPath    + "\""
                + " " + CommandLineParser.Parameter_PathOutput        + " \"" + outputPath  + "\""
                + " " + CommandLineParser.Parameter_ExitWhenDone;

            textBoxCommandLineExample.Text = commandText 
                + "\r\n\r\n" 
                +  appName + " installed in folder: " + AppSettings.LocalFileSystemPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //If there is a startup command, then follow it
            if (!string.IsNullOrWhiteSpace(AppSettings.CommandLine_Command))
            {
                RunStartupCommandLine();
            }
            else
            {
                //Load the normal application preferences
                AppPreferences_Load();
            }
        }



        /// <summary>
        /// Pushes updates to the UI
        /// </summary>
        /// <param name="statusUpdate"></param>
        void IShowLogs.NewLogResultsToShow(TaskStatusLogs statusUpdate)
        {
            textBoxStatus.Text = statusUpdate.StatusText;
            textBoxErrors.Text = statusUpdate.ErrorText;
        }

        private void btnProvisionFromAzureAd_Click(object sender, EventArgs e)
        {
            var statusLogs = new TaskStatusLogs();
            statusLogs.AddStatus("Starting...");
            UpdateStatusText(statusLogs, true);

            string pathSecrets = txtPathToSecrets.Text;
            if (!File.Exists(pathSecrets))
            {
                MessageBox.Show("Secrets file does not exist at specified path (" + pathSecrets + ")");
                return;
            }


            string pathProvisionPlan = txtPathToAzureAdProvisioningConfig.Text;
            if (!File.Exists(pathProvisionPlan))
            {
                MessageBox.Show("Config file does not exist at specified path (" + pathProvisionPlan + ")");
                return;
            }

            string pathOutput =
                    Path.Combine(
                        Path.GetDirectoryName(pathProvisionPlan),
                        "out");
            FileIOHelper.CreatePathIfNeeded(pathOutput);

            //Show the user a command line that they can use to run this same work
            GenerateProvisioningCommandLine(
                CommandLineParser.Command_ProvisionFromAzure,
                pathSecrets,
                pathProvisionPlan,
                pathOutput);

            //Run the work
            try
            {
                ProvisionFromAzureAd(
                    statusLogs,
                    pathSecrets,
                    txtPathToAzureAdProvisioningConfig.Text,
                    pathOutput);
            }
            catch (Exception exError)
            {
                MessageBox.Show("Error: " + exError.Message);
            }

            UpdateStatusText(statusLogs, true);

            //Open the file explorer to the output directory
            if (Directory.Exists(pathOutput))
            {
                System.Diagnostics.Process.Start(pathOutput);
            }

        }


        /// <summary>
        /// Provision a site based on a file based manifest
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProvisionSiteFromManifestFile_Click(object sender, EventArgs e)
        {
            var statusLogs = new TaskStatusLogs();
            statusLogs.AddStatus("Starting...");
            UpdateStatusText(statusLogs, true);

            string pathSecrets = txtPathToSecrets.Text;
            if(!File.Exists(pathSecrets))
            {
                MessageBox.Show("Secrets file does not exist at specified path (" + pathSecrets + ")");
                return;
            }

            string pathProvisionPlan = txtPathToFileProvisioningConfig.Text;
            if (!File.Exists(pathProvisionPlan))
            {
                MessageBox.Show("Config file does not exist at specified path (" + pathProvisionPlan + ")");
                return;
            }



            string pathOutput =
                    Path.Combine(
                        Path.GetDirectoryName(pathProvisionPlan),
                        "out");

            //Show the user a command line that they can use to run this same work
            GenerateProvisioningCommandLine(
                CommandLineParser.Command_ProvisionFromFileManifest,
                pathSecrets,
                pathProvisionPlan,
                pathOutput);

            //Run the work
            try
            {
                ProvisionFromFileManifest(statusLogs, pathSecrets, pathProvisionPlan, pathOutput);
            }
            catch (Exception ex)
            {
                statusLogs.AddError("Untrapped error: " + ex.Message);
            }

            //Update the status text to it's final state
            UpdateStatusText(statusLogs, true);

            //Open the file explorer to the output directory
            if(Directory.Exists(pathOutput))
            {
                System.Diagnostics.Process.Start(pathOutput);
            }

        }

        /// <summary>
        /// Run the provisioning pulling from AzureAD
        /// </summary>
        /// <param name="pathSecrets"></param>
        /// <param name="pathProvisionPlan"></param>
        private void ProvisionFromAzureAd(
            TaskStatusLogs statusLogs,
            string pathSecrets,
            string pathProvisionPlan,
            string pathOutputs)
        {
            //===========================================================================================
            //Create a place for out output files
            //===========================================================================================
            FileIOHelper.CreatePathIfNeeded(pathOutputs);

            AzureAdConfig configSignInAzure;
            ProvisionConfigExternalDirectorySync configGroupsMapping;

            //===========================================================================================
            //Get the sign in information
            //===========================================================================================
            try
            {
                //Load the config from the files
                configSignInAzure = new AzureAdConfig(pathSecrets);
            }
            catch(Exception exSignInConfig)
            {
                statusLogs.AddError("Error loading sign in config file");
                throw new Exception("813-1212: Error parsing sign in config, " + exSignInConfig.Message);
            }

            //===========================================================================================
            //Get the Groups/Roles mapping information 
            //===========================================================================================
            try
            {
                configGroupsMapping = new ProvisionConfigExternalDirectorySync(pathProvisionPlan);
            }
            catch(Exception exGroupsMapping)
            {
                statusLogs.AddError("Error loading sync groups provisioning file");
                throw new Exception("813-1214: Error parsing sync groups, " + exGroupsMapping.Message);
            }

            //===========================================================================================
            //Download all the data we need from Azure
            //===========================================================================================
            statusLogs.AddStatusHeader("Retrieving information from Azure AD");
            UpdateStatusText(statusLogs);
            var azureDownload = new AzureDownload(configSignInAzure, configGroupsMapping, this, statusLogs, null);
            try
            {
                azureDownload.Execute();
                //Sanity test
                IwsDiagnostics.Assert(azureDownload.IsExecuteComplete.Value, "813-834: Internal error. Async work still running");
            }
            catch(Exception exAzureDownload)
            {
                statusLogs.AddError("Error retrieving data from Azure AD");
                throw new Exception("813-0148: Error in Azure Download, " + exAzureDownload.Message);
            }

            //===========================================================================================
            //Write the provisioning manifest out to an intermediary file
            //===========================================================================================
            statusLogs.AddStatusHeader("Writing out manifest file for Tableau provisioning");
            UpdateStatusText(statusLogs);
            var outputProvisioningRoles = azureDownload.ProvisioningManifestResults;
            string provisioningManifest = Path.Combine(pathOutputs, "ProvisioningManifest.xml");
            try
            {
                outputProvisioningRoles.GenerateProvisioningManifestFile(provisioningManifest, configGroupsMapping);
            }
            catch (Exception exWriteProvisioningManifest)
            {
                statusLogs.AddError("Error creating provisioning manifest");
                throw new Exception("813-739: Error writing provisioning manifest, " + exWriteProvisioningManifest.Message);
            }

            //===========================================================================================
            //Provision the Tableau site using the manifest file we just created
            //===========================================================================================
            statusLogs.AddStatusHeader("Provision Tableau site using generated manifest file");
            UpdateStatusText(statusLogs);
            try
            {
                ProvisionFromFileManifest(statusLogs, pathSecrets, provisioningManifest, pathOutputs);
            }
            catch(Exception exProvisionSite)
            {
                statusLogs.AddError("Error provisioning Tableau Online site");
                throw new Exception("814-353: Error provisioning Tableau Online site, " + exProvisionSite.Message);
            }
        }

        /// <summary>
        /// Provision the site based on the provisioning manifest in a file
        /// </summary>
        /// <param name="statusLogs">Store status logs here</param>
        /// <param name="pathSecrets">Where the log in secrets are</param>
        /// <param name="pathProvisioningManifest">Where the provisioning steps are</param>
        /// <param name="outputPath">Where output files go</param>
        private void ProvisionFromFileManifest(TaskStatusLogs statusLogs, string pathSecrets, string pathProvisioningManifest, string outputPath)
        {
            //Load the config from the files
            var secretsConfig = new ProvisionConfigSiteAccess(pathSecrets);

            //Load the user provisioning instructions
            var provisionUsersInfo = new ProvisionUserInstructions(
                pathProvisioningManifest);

            var provisionSite = new ProvisionSite(secretsConfig, provisionUsersInfo, this, statusLogs);
            provisionSite.Execute();

            //---------------------------------------------------------------------
            //Generate an output file
            //---------------------------------------------------------------------
            FileIOHelper.CreatePathIfNeeded(outputPath);

            var outputFilePath = Path.Combine(outputPath, "ProvisionSiteOutput.csv");
            provisionSite.CSVResultsReport.GenerateCSVFile(outputFilePath);

            statusLogs.AddStatusHeader("Done!");
            ((IShowLogs)this).NewLogResultsToShow(statusLogs);
        }
    }
}
