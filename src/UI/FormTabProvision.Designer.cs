namespace OnlineContentDownloader
{
    partial class FormTabProvision
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainerStatus = new System.Windows.Forms.SplitContainer();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.textBoxErrors = new System.Windows.Forms.TextBox();
            this.chkVerboseLog = new System.Windows.Forms.CheckBox();
            this.txtPathToSecrets = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnProvisionSite = new System.Windows.Forms.Button();
            this.txtPathToFileProvisioningConfig = new System.Windows.Forms.TextBox();
            this.btnProvisionFromAzureAd = new System.Windows.Forms.Button();
            this.txtPathToAzureAdProvisioningConfig = new System.Windows.Forms.TextBox();
            this.textBoxCommandLineExample = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnCreateBackupManifestFile = new System.Windows.Forms.Button();
            this.txtPathToGenerateManifestFile = new System.Windows.Forms.TextBox();
            this.chkIgoreAllUsersGroup = new System.Windows.Forms.CheckBox();
            this.btnAzureAdGenerateManifestOnly = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatus)).BeginInit();
            this.splitContainerStatus.Panel1.SuspendLayout();
            this.splitContainerStatus.Panel2.SuspendLayout();
            this.splitContainerStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerStatus
            // 
            this.splitContainerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerStatus.Location = new System.Drawing.Point(6, 567);
            this.splitContainerStatus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainerStatus.Name = "splitContainerStatus";
            // 
            // splitContainerStatus.Panel1
            // 
            this.splitContainerStatus.Panel1.Controls.Add(this.textBoxStatus);
            // 
            // splitContainerStatus.Panel2
            // 
            this.splitContainerStatus.Panel2.Controls.Add(this.textBoxErrors);
            this.splitContainerStatus.Panel2.Controls.Add(this.chkVerboseLog);
            this.splitContainerStatus.Size = new System.Drawing.Size(1420, 309);
            this.splitContainerStatus.SplitterDistance = 905;
            this.splitContainerStatus.TabIndex = 18;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.BackColor = System.Drawing.Color.White;
            this.textBoxStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxStatus.Location = new System.Drawing.Point(0, 0);
            this.textBoxStatus.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(901, 304);
            this.textBoxStatus.TabIndex = 5;
            this.textBoxStatus.Text = "not yet started...";
            // 
            // textBoxErrors
            // 
            this.textBoxErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.textBoxErrors.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxErrors.Location = new System.Drawing.Point(3, 0);
            this.textBoxErrors.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxErrors.Multiline = true;
            this.textBoxErrors.Name = "textBoxErrors";
            this.textBoxErrors.ReadOnly = true;
            this.textBoxErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxErrors.Size = new System.Drawing.Size(508, 304);
            this.textBoxErrors.TabIndex = 8;
            this.textBoxErrors.Text = "no errors yet...";
            // 
            // chkVerboseLog
            // 
            this.chkVerboseLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkVerboseLog.AutoSize = true;
            this.chkVerboseLog.Checked = true;
            this.chkVerboseLog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVerboseLog.Location = new System.Drawing.Point(301, 271);
            this.chkVerboseLog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkVerboseLog.Name = "chkVerboseLog";
            this.chkVerboseLog.Size = new System.Drawing.Size(170, 23);
            this.chkVerboseLog.TabIndex = 11;
            this.chkVerboseLog.Text = "Verbose logging";
            this.chkVerboseLog.UseVisualStyleBackColor = true;
            // 
            // txtPathToSecrets
            // 
            this.txtPathToSecrets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathToSecrets.Location = new System.Drawing.Point(136, 4);
            this.txtPathToSecrets.Name = "txtPathToSecrets";
            this.txtPathToSecrets.Size = new System.Drawing.Size(1290, 26);
            this.txtPathToSecrets.TabIndex = 66;
            this.txtPathToSecrets.Text = "<<Path to the secrets file...>>";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 19);
            this.label2.TabIndex = 66;
            this.label2.Text = "Secrets path";
            // 
            // btnProvisionSite
            // 
            this.btnProvisionSite.BackColor = System.Drawing.Color.OrangeRed;
            this.btnProvisionSite.ForeColor = System.Drawing.Color.White;
            this.btnProvisionSite.Location = new System.Drawing.Point(6, 64);
            this.btnProvisionSite.Name = "btnProvisionSite";
            this.btnProvisionSite.Size = new System.Drawing.Size(328, 45);
            this.btnProvisionSite.TabIndex = 67;
            this.btnProvisionSite.Text = "Provision site: From file manifest";
            this.btnProvisionSite.UseVisualStyleBackColor = false;
            this.btnProvisionSite.Click += new System.EventHandler(this.btnProvisionSiteFromManifestFile_Click);
            // 
            // txtPathToFileProvisioningConfig
            // 
            this.txtPathToFileProvisioningConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathToFileProvisioningConfig.Location = new System.Drawing.Point(340, 65);
            this.txtPathToFileProvisioningConfig.Name = "txtPathToFileProvisioningConfig";
            this.txtPathToFileProvisioningConfig.Size = new System.Drawing.Size(1086, 26);
            this.txtPathToFileProvisioningConfig.TabIndex = 68;
            this.txtPathToFileProvisioningConfig.Text = "<<Path to the file based provisioning...>>";
            // 
            // btnProvisionFromAzureAd
            // 
            this.btnProvisionFromAzureAd.BackColor = System.Drawing.Color.OrangeRed;
            this.btnProvisionFromAzureAd.ForeColor = System.Drawing.Color.White;
            this.btnProvisionFromAzureAd.Location = new System.Drawing.Point(6, 136);
            this.btnProvisionFromAzureAd.Name = "btnProvisionFromAzureAd";
            this.btnProvisionFromAzureAd.Size = new System.Drawing.Size(328, 47);
            this.btnProvisionFromAzureAd.TabIndex = 69;
            this.btnProvisionFromAzureAd.Text = "Provision site: From Azure AD";
            this.btnProvisionFromAzureAd.UseVisualStyleBackColor = false;
            this.btnProvisionFromAzureAd.Click += new System.EventHandler(this.btnProvisionFromAzureAd_Click);
            // 
            // txtPathToAzureAdProvisioningConfig
            // 
            this.txtPathToAzureAdProvisioningConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathToAzureAdProvisioningConfig.Location = new System.Drawing.Point(340, 137);
            this.txtPathToAzureAdProvisioningConfig.Name = "txtPathToAzureAdProvisioningConfig";
            this.txtPathToAzureAdProvisioningConfig.Size = new System.Drawing.Size(1086, 26);
            this.txtPathToAzureAdProvisioningConfig.TabIndex = 70;
            this.txtPathToAzureAdProvisioningConfig.Text = "<<Path to the Azure based provisioning...>>";
            // 
            // textBoxCommandLineExample
            // 
            this.textBoxCommandLineExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommandLineExample.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.textBoxCommandLineExample.ForeColor = System.Drawing.Color.White;
            this.textBoxCommandLineExample.Location = new System.Drawing.Point(6, 404);
            this.textBoxCommandLineExample.Multiline = true;
            this.textBoxCommandLineExample.Name = "textBoxCommandLineExample";
            this.textBoxCommandLineExample.ReadOnly = true;
            this.textBoxCommandLineExample.Size = new System.Drawing.Size(1420, 115);
            this.textBoxCommandLineExample.TabIndex = 74;
            this.textBoxCommandLineExample.Text = "Example command line will get generated here";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 382);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(828, 19);
            this.label1.TabIndex = 75;
            this.label1.Text = "Generated command line (copy/paste from here if you want to run this from the com" +
    "mand line)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 540);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 19);
            this.label3.TabIndex = 76;
            this.label3.Text = "Output logs";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(1178, 540);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(248, 19);
            this.label4.TabIndex = 77;
            this.label4.Text = "Errors / Unexpected";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(181, 28);
            this.label5.TabIndex = 78;
            this.label5.Text = "Choose action";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(159, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 28);
            this.label6.TabIndex = 79;
            this.label6.Text = "OR";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(159, 268);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 28);
            this.label7.TabIndex = 80;
            this.label7.Text = "OR";
            // 
            // btnCreateBackupManifestFile
            // 
            this.btnCreateBackupManifestFile.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnCreateBackupManifestFile.ForeColor = System.Drawing.Color.White;
            this.btnCreateBackupManifestFile.Location = new System.Drawing.Point(6, 289);
            this.btnCreateBackupManifestFile.Name = "btnCreateBackupManifestFile";
            this.btnCreateBackupManifestFile.Size = new System.Drawing.Size(328, 47);
            this.btnCreateBackupManifestFile.TabIndex = 71;
            this.btnCreateBackupManifestFile.Text = "Create provisioning manifest: From current Online site";
            this.btnCreateBackupManifestFile.UseVisualStyleBackColor = false;
            this.btnCreateBackupManifestFile.Click += new System.EventHandler(this.btnCreateBackupManifestFile_Click);
            // 
            // txtPathToGenerateManifestFile
            // 
            this.txtPathToGenerateManifestFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathToGenerateManifestFile.Location = new System.Drawing.Point(340, 290);
            this.txtPathToGenerateManifestFile.Name = "txtPathToGenerateManifestFile";
            this.txtPathToGenerateManifestFile.Size = new System.Drawing.Size(1081, 26);
            this.txtPathToGenerateManifestFile.TabIndex = 72;
            this.txtPathToGenerateManifestFile.Text = "<<Path to write provisioning manifest file>>";
            // 
            // chkIgoreAllUsersGroup
            // 
            this.chkIgoreAllUsersGroup.AutoSize = true;
            this.chkIgoreAllUsersGroup.Checked = true;
            this.chkIgoreAllUsersGroup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIgoreAllUsersGroup.Location = new System.Drawing.Point(344, 319);
            this.chkIgoreAllUsersGroup.Name = "chkIgoreAllUsersGroup";
            this.chkIgoreAllUsersGroup.Size = new System.Drawing.Size(467, 23);
            this.chkIgoreAllUsersGroup.TabIndex = 81;
            this.chkIgoreAllUsersGroup.Text = "Ignore \"All Users\" group in export (recommended)";
            this.chkIgoreAllUsersGroup.UseVisualStyleBackColor = true;
            // 
            // btnAzureAdGenerateManifestOnly
            // 
            this.btnAzureAdGenerateManifestOnly.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnAzureAdGenerateManifestOnly.ForeColor = System.Drawing.Color.White;
            this.btnAzureAdGenerateManifestOnly.Location = new System.Drawing.Point(6, 209);
            this.btnAzureAdGenerateManifestOnly.Name = "btnAzureAdGenerateManifestOnly";
            this.btnAzureAdGenerateManifestOnly.Size = new System.Drawing.Size(328, 47);
            this.btnAzureAdGenerateManifestOnly.TabIndex = 82;
            this.btnAzureAdGenerateManifestOnly.Text = "Create provisioning manifest: From Azure AD (not deploy)";
            this.btnAzureAdGenerateManifestOnly.UseVisualStyleBackColor = false;
            this.btnAzureAdGenerateManifestOnly.Click += new System.EventHandler(this.btnAzureAdGenerateManifestOnly_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(159, 186);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(38, 28);
            this.label8.TabIndex = 83;
            this.label8.Text = "OR";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(336, 223);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(333, 19);
            this.label9.TabIndex = 84;
            this.label9.Text = "(uses Azure AD path specified above)";
            // 
            // FormTabProvision
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1433, 882);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnAzureAdGenerateManifestOnly);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.chkIgoreAllUsersGroup);
            this.Controls.Add(this.txtPathToGenerateManifestFile);
            this.Controls.Add(this.btnCreateBackupManifestFile);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxCommandLineExample);
            this.Controls.Add(this.txtPathToAzureAdProvisioningConfig);
            this.Controls.Add(this.btnProvisionFromAzureAd);
            this.Controls.Add(this.txtPathToFileProvisioningConfig);
            this.Controls.Add(this.btnProvisionSite);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPathToSecrets);
            this.Controls.Add(this.splitContainerStatus);
            this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "FormTabProvision";
            this.Text = "Online - TabProvision, site user/roles/auth/groups provisioning (v1.011)";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainerStatus.Panel1.ResumeLayout(false);
            this.splitContainerStatus.Panel1.PerformLayout();
            this.splitContainerStatus.Panel2.ResumeLayout(false);
            this.splitContainerStatus.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerStatus)).EndInit();
            this.splitContainerStatus.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainerStatus;
        private System.Windows.Forms.TextBox textBoxErrors;
        private System.Windows.Forms.CheckBox chkVerboseLog;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TextBox txtPathToSecrets;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnProvisionSite;
        private System.Windows.Forms.TextBox txtPathToFileProvisioningConfig;
        private System.Windows.Forms.Button btnProvisionFromAzureAd;
        private System.Windows.Forms.TextBox txtPathToAzureAdProvisioningConfig;
        private System.Windows.Forms.TextBox textBoxCommandLineExample;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnCreateBackupManifestFile;
        private System.Windows.Forms.TextBox txtPathToGenerateManifestFile;
        private System.Windows.Forms.CheckBox chkIgoreAllUsersGroup;
        private System.Windows.Forms.Button btnAzureAdGenerateManifestOnly;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
    }
}

