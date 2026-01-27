namespace SDRSharp.RTLTCP
{
    partial class RTLTcpSettings
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
            FreeRessources();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.MainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.listViewConsole = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxServer = new System.Windows.Forms.GroupBox();
            this.groupBoxTCPServer = new System.Windows.Forms.GroupBox();
            this.buttonMessagesToClipboard = new System.Windows.Forms.Button();
            this.checkBoxSubFolder = new System.Windows.Forms.CheckBox();
            this.buttonClearMessages = new System.Windows.Forms.Button();
            this.labelFile = new System.Windows.Forms.Label();
            this.labelFreqForce = new System.Windows.Forms.Label();
            this.labelSRForce = new System.Windows.Forms.Label();
            this.buttonStartRadio = new System.Windows.Forms.Button();
            this.labelNumFile = new System.Windows.Forms.Label();
            this.labelNbSendingForAllFiles = new System.Windows.Forms.Label();
            this.labelNbSendingForEachFile = new System.Windows.Forms.Label();
            this.textBoxTempoBetweenFile = new System.Windows.Forms.TextBox();
            this.textBoxNEmissionForAllFiles = new System.Windows.Forms.TextBox();
            this.labelTempoBetweenFile = new System.Windows.Forms.Label();
            this.textBoxNEmissionForEachFile = new System.Windows.Forms.TextBox();
            this.labelNEmissionForAllFiles = new System.Windows.Forms.Label();
            this.labelNEmissionForEachFile = new System.Windows.Forms.Label();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.folderBrowserDialogChooseFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.MainTableLayoutPanel.SuspendLayout();
            this.groupBoxServer.SuspendLayout();
            this.groupBoxTCPServer.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTableLayoutPanel
            // 
            this.MainTableLayoutPanel.ColumnCount = 1;
            this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainTableLayoutPanel.Controls.Add(this.listViewConsole, 0, 1);
            this.MainTableLayoutPanel.Controls.Add(this.groupBoxServer, 0, 0);
            this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
            this.MainTableLayoutPanel.RowCount = 2;
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayoutPanel.Size = new System.Drawing.Size(488, 535);
            this.MainTableLayoutPanel.TabIndex = 25;
            this.MainTableLayoutPanel.Resize += new System.EventHandler(this.MainTableLayoutPanel_SizeChanged);
            // 
            // listViewConsole
            // 
            this.listViewConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewConsole.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewConsole.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewConsole.HideSelection = false;
            this.listViewConsole.Location = new System.Drawing.Point(3, 316);
            this.listViewConsole.Name = "listViewConsole";
            this.listViewConsole.Size = new System.Drawing.Size(482, 216);
            this.listViewConsole.TabIndex = 34;
            this.listViewConsole.UseCompatibleStateImageBehavior = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            // 
            // groupBoxServer
            // 
            this.groupBoxServer.AutoSize = true;
            this.groupBoxServer.Controls.Add(this.groupBoxTCPServer);
            this.groupBoxServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxServer.Location = new System.Drawing.Point(3, 3);
            this.groupBoxServer.Name = "groupBoxServer";
            this.groupBoxServer.Size = new System.Drawing.Size(482, 307);
            this.groupBoxServer.TabIndex = 23;
            this.groupBoxServer.TabStop = false;
            // 
            // groupBoxTCPServer
            // 
            this.groupBoxTCPServer.Controls.Add(this.buttonMessagesToClipboard);
            this.groupBoxTCPServer.Controls.Add(this.checkBoxSubFolder);
            this.groupBoxTCPServer.Controls.Add(this.buttonClearMessages);
            this.groupBoxTCPServer.Controls.Add(this.labelFile);
            this.groupBoxTCPServer.Controls.Add(this.labelFreqForce);
            this.groupBoxTCPServer.Controls.Add(this.labelSRForce);
            this.groupBoxTCPServer.Controls.Add(this.buttonStartRadio);
            this.groupBoxTCPServer.Controls.Add(this.labelNumFile);
            this.groupBoxTCPServer.Controls.Add(this.labelNbSendingForAllFiles);
            this.groupBoxTCPServer.Controls.Add(this.labelNbSendingForEachFile);
            this.groupBoxTCPServer.Controls.Add(this.textBoxTempoBetweenFile);
            this.groupBoxTCPServer.Controls.Add(this.textBoxNEmissionForAllFiles);
            this.groupBoxTCPServer.Controls.Add(this.labelTempoBetweenFile);
            this.groupBoxTCPServer.Controls.Add(this.textBoxNEmissionForEachFile);
            this.groupBoxTCPServer.Controls.Add(this.labelNEmissionForAllFiles);
            this.groupBoxTCPServer.Controls.Add(this.labelNEmissionForEachFile);
            this.groupBoxTCPServer.Location = new System.Drawing.Point(9, 19);
            this.groupBoxTCPServer.Name = "groupBoxTCPServer";
            this.groupBoxTCPServer.Size = new System.Drawing.Size(311, 269);
            this.groupBoxTCPServer.TabIndex = 6;
            this.groupBoxTCPServer.TabStop = false;
            this.groupBoxTCPServer.Text = "Files replay for Source RTL-SDR(TCP)";
            // 
            // buttonMessagesToClipboard
            // 
            this.buttonMessagesToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonMessagesToClipboard.AutoSize = true;
            this.buttonMessagesToClipboard.Location = new System.Drawing.Point(225, 227);
            this.buttonMessagesToClipboard.Name = "buttonMessagesToClipboard";
            this.buttonMessagesToClipboard.Size = new System.Drawing.Size(80, 36);
            this.buttonMessagesToClipboard.TabIndex = 37;
            this.buttonMessagesToClipboard.Text = "Messages to \r\nclipboard";
            this.buttonMessagesToClipboard.UseVisualStyleBackColor = false;
            this.buttonMessagesToClipboard.Click += new System.EventHandler(this.ButtonMessagesToClipboard_Click);
            // 
            // checkBoxSubFolder
            // 
            this.checkBoxSubFolder.AutoSize = true;
            this.checkBoxSubFolder.Location = new System.Drawing.Point(79, 82);
            this.checkBoxSubFolder.Name = "checkBoxSubFolder";
            this.checkBoxSubFolder.Size = new System.Drawing.Size(82, 17);
            this.checkBoxSubFolder.TabIndex = 36;
            this.checkBoxSubFolder.Text = "Sub Folders";
            this.checkBoxSubFolder.UseVisualStyleBackColor = true;
            // 
            // buttonClearMessages
            // 
            this.buttonClearMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonClearMessages.AutoSize = true;
            this.buttonClearMessages.Location = new System.Drawing.Point(151, 227);
            this.buttonClearMessages.Name = "buttonClearMessages";
            this.buttonClearMessages.Size = new System.Drawing.Size(68, 36);
            this.buttonClearMessages.TabIndex = 35;
            this.buttonClearMessages.Text = "    Clear\r\n Messages";
            this.buttonClearMessages.UseVisualStyleBackColor = true;
            this.buttonClearMessages.Click += new System.EventHandler(this.ButtonClearMessages_Click);
            // 
            // labelFile
            // 
            this.labelFile.AutoSize = true;
            this.labelFile.Location = new System.Drawing.Point(7, 111);
            this.labelFile.Name = "labelFile";
            this.labelFile.Size = new System.Drawing.Size(28, 13);
            this.labelFile.TabIndex = 34;
            this.labelFile.Text = "Files";
            // 
            // labelFreqForce
            // 
            this.labelFreqForce.AutoSize = true;
            this.labelFreqForce.Location = new System.Drawing.Point(82, 62);
            this.labelFreqForce.Name = "labelFreqForce";
            this.labelFreqForce.Size = new System.Drawing.Size(177, 13);
            this.labelFreqForce.TabIndex = 33;
            this.labelFreqForce.Text = "Forced frequency if not  in the name";
            // 
            // labelSRForce
            // 
            this.labelSRForce.AutoSize = true;
            this.labelSRForce.Location = new System.Drawing.Point(82, 23);
            this.labelSRForce.Name = "labelSRForce";
            this.labelSRForce.Size = new System.Drawing.Size(184, 13);
            this.labelSRForce.TabIndex = 33;
            this.labelSRForce.Text = "Forced sample rate if not  in the name";
            // 
            // buttonStartRadio
            // 
            this.buttonStartRadio.Location = new System.Drawing.Point(6, 227);
            this.buttonStartRadio.Name = "buttonStartRadio";
            this.buttonStartRadio.Size = new System.Drawing.Size(67, 36);
            this.buttonStartRadio.TabIndex = 30;
            this.buttonStartRadio.Text = "Start radio";
            this.buttonStartRadio.UseVisualStyleBackColor = true;
            this.buttonStartRadio.Click += new System.EventHandler(this.ButtonStartRadio_Click);
            // 
            // labelNumFile
            // 
            this.labelNumFile.AutoSize = true;
            this.labelNumFile.Location = new System.Drawing.Point(49, 111);
            this.labelNumFile.Name = "labelNumFile";
            this.labelNumFile.Size = new System.Drawing.Size(24, 13);
            this.labelNumFile.TabIndex = 25;
            this.labelNumFile.Text = "0/0";
            // 
            // labelNbSendingForAllFiles
            // 
            this.labelNbSendingForAllFiles.AutoSize = true;
            this.labelNbSendingForAllFiles.Location = new System.Drawing.Point(199, 166);
            this.labelNbSendingForAllFiles.Name = "labelNbSendingForAllFiles";
            this.labelNbSendingForAllFiles.Size = new System.Drawing.Size(24, 13);
            this.labelNbSendingForAllFiles.TabIndex = 24;
            this.labelNbSendingForAllFiles.Text = "0/0";
            // 
            // labelNbSendingForEachFile
            // 
            this.labelNbSendingForEachFile.AutoSize = true;
            this.labelNbSendingForEachFile.Location = new System.Drawing.Point(199, 146);
            this.labelNbSendingForEachFile.Name = "labelNbSendingForEachFile";
            this.labelNbSendingForEachFile.Size = new System.Drawing.Size(24, 13);
            this.labelNbSendingForEachFile.TabIndex = 23;
            this.labelNbSendingForEachFile.Text = "0/0";
            // 
            // textBoxTempoBetweenFile
            // 
            this.textBoxTempoBetweenFile.Location = new System.Drawing.Point(145, 192);
            this.textBoxTempoBetweenFile.Name = "textBoxTempoBetweenFile";
            this.textBoxTempoBetweenFile.Size = new System.Drawing.Size(48, 20);
            this.textBoxTempoBetweenFile.TabIndex = 11;
            this.textBoxTempoBetweenFile.Text = "0";
            // 
            // textBoxNEmissionForAllFiles
            // 
            this.textBoxNEmissionForAllFiles.Location = new System.Drawing.Point(145, 166);
            this.textBoxNEmissionForAllFiles.Name = "textBoxNEmissionForAllFiles";
            this.textBoxNEmissionForAllFiles.Size = new System.Drawing.Size(48, 20);
            this.textBoxNEmissionForAllFiles.TabIndex = 15;
            this.textBoxNEmissionForAllFiles.Text = "1";
            // 
            // labelTempoBetweenFile
            // 
            this.labelTempoBetweenFile.AutoSize = true;
            this.labelTempoBetweenFile.Location = new System.Drawing.Point(7, 199);
            this.labelTempoBetweenFile.Name = "labelTempoBetweenFile";
            this.labelTempoBetweenFile.Size = new System.Drawing.Size(121, 13);
            this.labelTempoBetweenFile.TabIndex = 10;
            this.labelTempoBetweenFile.Text = "Delay between files(ms.)";
            // 
            // textBoxNEmissionForEachFile
            // 
            this.textBoxNEmissionForEachFile.Location = new System.Drawing.Point(145, 143);
            this.textBoxNEmissionForEachFile.Name = "textBoxNEmissionForEachFile";
            this.textBoxNEmissionForEachFile.Size = new System.Drawing.Size(48, 20);
            this.textBoxNEmissionForEachFile.TabIndex = 14;
            this.textBoxNEmissionForEachFile.Text = "1";
            // 
            // labelNEmissionForAllFiles
            // 
            this.labelNEmissionForAllFiles.AutoSize = true;
            this.labelNEmissionForAllFiles.Location = new System.Drawing.Point(7, 173);
            this.labelNEmissionForAllFiles.Name = "labelNEmissionForAllFiles";
            this.labelNEmissionForAllFiles.Size = new System.Drawing.Size(112, 13);
            this.labelNEmissionForAllFiles.TabIndex = 13;
            this.labelNEmissionForAllFiles.Text = "N emissions for all files";
            // 
            // labelNEmissionForEachFile
            // 
            this.labelNEmissionForEachFile.AutoSize = true;
            this.labelNEmissionForEachFile.ForeColor = System.Drawing.SystemColors.WindowText;
            this.labelNEmissionForEachFile.Location = new System.Drawing.Point(7, 150);
            this.labelNEmissionForEachFile.Name = "labelNEmissionForEachFile";
            this.labelNEmissionForEachFile.Size = new System.Drawing.Size(121, 13);
            this.labelNEmissionForEachFile.TabIndex = 12;
            this.labelNEmissionForEachFile.Text = "N emissions for each file";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 1000;
            this.refreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // RTLTcpSettings
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(488, 535);
            this.Controls.Add(this.MainTableLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RTLTcpSettings";
            this.ShowInTaskbar = false;
            this.Text = "Files Replay";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RTLTcpSettings_FormClosing);
            this.Load += new System.EventHandler(this.RTLTcpSettings_Load);
            this.VisibleChanged += new System.EventHandler(this.RTLTcpSettings_VisibleChanged);
            this.MainTableLayoutPanel.ResumeLayout(false);
            this.MainTableLayoutPanel.PerformLayout();
            this.groupBoxServer.ResumeLayout(false);
            this.groupBoxServer.PerformLayout();
            this.groupBoxTCPServer.ResumeLayout(false);
            this.groupBoxTCPServer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel MainTableLayoutPanel;
        private System.Windows.Forms.GroupBox groupBoxServer;
        private System.Windows.Forms.GroupBox groupBoxTCPServer;
        private System.Windows.Forms.Label labelNumFile;
        private System.Windows.Forms.Label labelNbSendingForAllFiles;
        private System.Windows.Forms.Label labelNbSendingForEachFile;
        private System.Windows.Forms.TextBox textBoxTempoBetweenFile;
        private System.Windows.Forms.TextBox textBoxNEmissionForAllFiles;
        private System.Windows.Forms.Label labelTempoBetweenFile;
        private System.Windows.Forms.TextBox textBoxNEmissionForEachFile;
        private System.Windows.Forms.Label labelNEmissionForAllFiles;
        private System.Windows.Forms.Label labelNEmissionForEachFile;
        private System.Windows.Forms.Button buttonStartRadio;
        private System.Windows.Forms.Label labelSRForce;
        private System.Windows.Forms.Label labelFreqForce;
        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.ListView listViewConsole;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonClearMessages;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.CheckBox checkBoxSubFolder;
        private System.Windows.Forms.Button buttonMessagesToClipboard;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogChooseFolder;
    }
}