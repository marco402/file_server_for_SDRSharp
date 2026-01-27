/* Written by Marc Prieur (marco40_github@sfr.fr)
                                   FormSendFiles.cs 
                                   project RTLTCP
 **************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license: 
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.

 model with https://github.com/sobieh/sdrsharp_rtl_tcp_direct/tree/master
  **********************************************************************************/
/*
  This dll replace SDRSharp.RTLTCP.dll with SDRSharp version from 1632 to 1732.

Multiple file replay for SDRSharp RTL-SDR source (TCP).

To use this software, a version of SDRSharp must be used where SDRSharp.RTLTCP.dll is located and replace this file.

For future versions of SDRSharp, it would be interesting to have a "custom source.

In this case, only replace name dll(SDRSharp.RTLTCP.dll), nameSpace (SDRSharp.RTLTCP) and name class (RtlTcpIO)

"I used version 1732 available for download at this address:
 https://archive.org/download/SDRSharp_Collection
 
 The data is sent internally to SDRSharp without network connection.
 
 The selected files are either .wav (except RF64) or any other 8-bit IQ file (e.g., .cu8 from RTL_433).
 
 If the sample rate is not present in the name, the forced sample rate will be used.
    if it is empty, the file will not be used.
 If the frequency is not present in the name, the forced frequency will be used.
    if it is empty, file is to send with frequency =0.
 For 1 start radio, it is necessary to select 1 sample rate, the frequency will be set for each file.
 
 As for the network version:
	-Number of emissions for each file.
	-Number of emissions for all files.
	-Tempo between 2 emissions.

*******************************************************************************************
use of BetterFolderBrowser --> https://github.com/Willy-Kimura/BetterFolderBrowser/tree/master

Copyright (c) 2022 Willy Kimura

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions
of the Software.

*******************************************************************************************
 */

using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ToolTip = System.Windows.Forms.ToolTip;

namespace SDRSharp.RTLTCP
{
    public partial class RTLTcpSettings : Form
    {
        #region declare
        private WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser betterFolderBrowser;
        private readonly RtlTcpIO _owner;

        //array of selected files
        private string[] Files;
        //listOfListFiles  file list array sorted by sample rate to speed up processing towards SDRSharp. 
        private readonly List<Dictionary<string, InfoFile>> listOfListFiles = new List<Dictionary<string, InfoFile>>();
        //list sample rate in SDRSharp
        private readonly List<int> sampleRate = new List<int>();
        //List frequency found in the files
        private List<long> listFrequency = new List<long>();
        //list sample rate found in the files + sampleRateForce if > 0
        private readonly List<int> usedListSampleRate = new List<int>();
        private ColorRadioButton[] radioButtonsSR;
        private ColorLabel[] labelNbFile;
        private string sampleRateForce = string.Empty;
        private string frequencyForce = string.Empty;
        private string memoFolder = "C:\\";
        private Int32 cpFilesWithoutFrequency = 0;
        private ColorButton buttonFilesSelect;
        private ColorButton buttonFolderSelect;
        private ColorGroupBox groupBoxRbSr;
        //private ColorComboBox comboBoxForceFrequency;
        private ColorComboBoxFakeDisabled comboBoxForceFrequency;
        private ColorComboBoxFakeDisabled comboBoxForceSampleRate;
        #endregion
        #region init end
        public RTLTcpSettings(RtlTcpIO owner)
        {
            _owner = owner;
            _owner.MessageReceived += Server_MessageReceived;
            InitializeComponent();
            InitColorControlForDisabled();
            InitVirtualListView();
            refreshTimer.Interval = 1000;
            ResetAllControlsBackColor(this);
            GetSetting();
            sampleRate.Add(250000);
            sampleRate.Add(900000);
            sampleRate.Add(1024000);
            sampleRate.Add(1400000);
            sampleRate.Add(1800000);
            sampleRate.Add(1920000);
            sampleRate.Add(2048000);
            sampleRate.Add(2400000);
            sampleRate.Add(2800000);
            sampleRate.Add(3200000);
            string memoText = comboBoxForceSampleRate.Text;
            foreach (int sr in sampleRate)
            {
                _ = comboBoxForceSampleRate.Items.Add(sr.ToString());
            }
            comboBoxForceSampleRate.Text = memoText;
            ToolTip ttbuttonChooseFiles = new ToolTip();
            ttbuttonChooseFiles.SetToolTip(buttonFilesSelect, "Choose file(s) Wav or/and raw IQ.");
            ToolTip ttbuttonChooseFolder = new ToolTip();
            ttbuttonChooseFolder.SetToolTip(buttonFolderSelect, "Choose folder (and subFolder if checked).");
            ToolTip ttcomboBoxForceSampleRate = new ToolTip();
            ttcomboBoxForceSampleRate.SetToolTip(comboBoxForceSampleRate, "If empty:Files without sample rate in the name will be ignored.");
            ToolTip ttcomboBoxForceFrequency = new ToolTip();
            ttcomboBoxForceFrequency.SetToolTip(comboBoxForceFrequency, "If empty:Files without frequency in the name will be 0hz.");
            buttonStartRadio.Enabled = true;
            string ver = Assembly.GetExecutingAssembly().FullName.Remove(Assembly.GetExecutingAssembly().FullName.Length - 38, 38);
            Text = $"File server {ver}";
            this.betterFolderBrowser = new WK.Libraries.BetterFolderBrowserNS.BetterFolderBrowser();
        }
        private void GetSetting()
        {
            textBoxNEmissionForEachFile.Text = Utils.GetStringSetting("RTLTCP.NEmEachFile", "1");
            textBoxNEmissionForAllFiles.Text = Utils.GetStringSetting("RTLTCP.NEmAllFile", "2");
            textBoxTempoBetweenFile.Text = Utils.GetStringSetting("RTLTCP.TempoBetwFile", "10");
            comboBoxForceSampleRate.Items.Add(Utils.GetStringSetting("RTLTCP.SampleRateForce", "1024000"));
            comboBoxForceSampleRate.SelectedItem = comboBoxForceSampleRate.Items[0];
            comboBoxForceFrequency.Items.Add(Utils.GetStringSetting("RTLTCP.FrequencyForce", "100000000"));
            comboBoxForceFrequency.SelectedItem = comboBoxForceFrequency.Items[0];
        }
        private void RTLTcpSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        private void SaveSetting()
        {
            Utils.SaveSetting("RTLTCP.NEmEachFile", textBoxNEmissionForEachFile.Text);
            Utils.SaveSetting("RTLTCP.NEmAllFile", textBoxNEmissionForAllFiles.Text);
            Utils.SaveSetting("RTLTCP.TempoBetwFile", textBoxTempoBetweenFile.Text);

            if (comboBoxForceSampleRate.Text.Trim() != "")
            {
                Utils.SaveSetting("RTLTCP.SampleRateForce", comboBoxForceSampleRate.Text);
            }

            if (comboBoxForceFrequency.Text.Trim() != "") 
            {
                Utils.SaveSetting("RTLTCP.FrequencyForce", comboBoxForceFrequency.Text);
            }
         }
        #endregion
        #region events form
        private void ButtonFilesSelect_Click(object sender, EventArgs e)
        {
            if (!GetFiles(ref Files, "*.*"))
            {
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            FileInfo f = new FileInfo(Files[0]);
            AddMessage($"{f.DirectoryName}\n");
            TreatFiles(Files);
        }
        private int nbFilesWithSampleRateAndFrequency = 0;
        private Boolean TreatFiles(string[] allFiles)
        {
            _owner.CanTune = false;
            _owner.Samplerate = -2;
            usedListSampleRate?.Clear();
            cpFilesWithoutFrequency = 0;
            listOfListFiles?.Clear();

            TriSampleRate(listOfListFiles, allFiles);

            if(usedListSampleRate.Count() == 0 && sampleRateForce == String.Empty || listOfListFiles.Count()==0)
            {
                ClearSampleRateUsedList();
                labelNumFile.Text = "0/0";
                this.Cursor = Cursors.Default;
                return false;
            }

            DisplayResult();

            this.Cursor = Cursors.Default;
            return true;
        }
        private void DisplayResult()
        {
            DisplaySampleRateUsedList(usedListSampleRate);
            if (labelNbFile.Count() > 0)
            {
                nbFilesWithSampleRateAndFrequency = 0;
                int j = 0;
                foreach (Dictionary<string, InfoFile> sr in listOfListFiles)
                {
                    int NBFilesKept = 0;
                    if (sr.Count() > 0)
                    {
                        if (!(sr.ElementAt(0).Value.SampleRate == "-1"))
                        {
                            nbFilesWithSampleRateAndFrequency += sr.Count();
                            foreach (KeyValuePair<string, InfoFile> ss in sr)
                                if (!(ss.Value.Frequency == 0 && frequencyForce == string.Empty))
                                    NBFilesKept += 1;
                        }
                    }
                    if (NBFilesKept > 0)
                    {
                        labelNbFile[j].Text = NBFilesKept.ToString();
                        j += 1;
                    }
                  }
                if(cpFilesWithoutFrequency > 0)
                    _ = MessageBox.Show($"You have {cpFilesWithoutFrequency} files without frequency; enter a forced frequency, otherwise they will be transmitted with a frequency of 0.", "Select files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            int nbFile = GetNbKeptFiles();
            labelNumFile.Text = nbFilesWithSampleRateAndFrequency.ToString() + "/" + nbFile.ToString();
            _owner.CanTune = true;
        }
        private int GetNbKeptFiles()
        {
            int nbFile = 0;
            int indice = 0;
            //
            foreach (ColorLabel Cl in labelNbFile)
            {
                if(Cl.Text=="0")
                    radioButtonsSR[indice].Enabled = false;
                else
                    nbFile += Int32.Parse(Cl.Text);
                indice += 1;
            }
            return nbFile;
        }
        private void ButtonFolderSelect_Click(object sender, EventArgs e)
        {
            betterFolderBrowser.RootFolder = memoFolder;
            betterFolderBrowser.Multiselect = false;
            string[] folder = null;
            if (betterFolderBrowser.ShowDialog() == DialogResult.OK)
                folder = betterFolderBrowser.SelectedPaths;

            if (folder == null)
                return;
            memoFolder = folder[0];
            this.Cursor = Cursors.WaitCursor;
            Files = checkBoxSubFolder.Checked
                ? Directory.GetFiles(folder[0], "*.*", SearchOption.AllDirectories)
                : Directory.GetFiles(folder[0], "*.*", SearchOption.TopDirectoryOnly);
            if (Files == null)
            {
                this.Cursor = Cursors.Default;
                return;
            }
            AddMessage($"{folder[0]}\n");
            TreatFiles(Files);
        }
        private void ButtonStartRadio_Click(object sender, EventArgs e)
        {
            if(buttonStartRadio.Text == ClassConstMessage.STARTRADIO)
            {

               if (TestsOnStart())
                {
                    SaveSetting();
                    _owner.StartRadio();
                }
                else
                    return;

                if (sampleRateForce != comboBoxForceSampleRate.Text.Trim())
                {
                    _ = MessageBox.Show("The sample rate changed->rescan the files", "Rescan files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TreatFiles(Files);
                    return;
                }
 
            }
            else if(buttonStartRadio.Text == ClassConstMessage.STOPRADIO)
            {
                StopRd();
            }
           else if(buttonStartRadio.Text == ClassConstMessage.WAITFILES)
           {
                if(Files==null)
                    _ = MessageBox.Show("Choose a folder or files and after choose a sample rate", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    _ = MessageBox.Show("Choose a sample rate", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
           }
        }
        private int indiceNbFiles = 0;
        private void RadioButtonsSR_CheckedChanged(object sender, EventArgs e)
        {
            indiceNbFiles = 0;
            foreach (System.Windows.Forms.RadioButton sr in radioButtonsSR)
            {
                if (sr.Checked)
                {
                    _owner.Samplerate = (int)sr.Tag;  // not 0 only for init change after in start
                    labelNumFile.Text = "0/" + labelNbFile[indiceNbFiles].Text;
                    return;
                }
                indiceNbFiles += 1;
            }
        }
        private void ComboBoxForceSampleRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // bad char
            }
        }
        private void Server_MessageReceived(object sender, string message)
        {
            _ = listViewConsole.Invoke((MethodInvoker)(() =>
            {
                AddMessage(message);
            }));
        }
        private void ComboBoxForceFrequency_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // bad char
            }
        }
        private void ButtonMessagesToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            string text = string.Empty;
            if (listViewConsole.Items.Count > 1)
            {
                for (int item = 0; item < listViewConsole.Items.Count; item++)
                    text += listViewConsole.Items[item].Text + "\n";
                Clipboard.SetText(text);
            }
        }
        #endregion
        #region  listViewConsole 
        private List<ListViewItem> cacheLines;
        private bool listViewConsoleFull = false;
        private string memoLine = string.Empty;
        private void InitVirtualListView()
        {
            _ = typeof(Control).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, listViewConsole, new object[] { true });
            SuspendLayout();
            SDRSharp.RTLTCP.ClassFunctionsVirtualListView.InitListView(listViewConsole);
            listViewConsole.GridLines = false;
            listViewConsole.FullRowSelect = false;
            listViewConsole.View = View.Details;   //hide column header
            listViewConsole.MultiSelect = true;
            listViewConsole.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(ListViewConsole_RetrieveVirtualItem);
            ClearListViewConsole();
            ResumeLayout(true);
        }
        internal void ListViewConsole_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (cacheLines != null)
            {
                try
                {
                    if (e.ItemIndex >= 0)
                    {
                        ListViewItem lvi = cacheLines[e.ItemIndex];
                        if (lvi != null)
                        {
                            e.Item = lvi;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, "Error fct(listViewConsole_RetrieveVirtualItem)");
                }
            }
        }
        internal bool WriteLine(Dictionary<string, string> listData)
        {
            string theLine = string.Empty;
            foreach (KeyValuePair<string, string> _line in listData)
            {
                theLine += _line.Key + _line.Value;
            }

            return WriteLine(theLine);
        }
        internal bool WriteLine(string theLine)
        {
            int cptLine = 0;
            bool retour = false;
            if (cacheLines == null)
            {
                cacheLines = new List<ListViewItem>();
            }
            SuspendLayout();
            listViewConsole.BeginUpdate();
            memoLine += theLine;
            string[] newLine = memoLine.Split((char)0x0a);
            memoLine = string.Empty;
            int lastWord = 0;
            if (newLine[newLine.Length - 1].Length > 0)
            {
                memoLine = newLine[newLine.Length - 1];
                lastWord = 1;
            }
            for (int i = 0; i < newLine.Length - lastWord; i++)
            {
                if (newLine[i].Length > 0)
                {
                    ListViewItem line = new ListViewItem(newLine[i]);
                    cacheLines.Add(line);
                    cptLine++;
                }
            }
            listViewConsole.VirtualListSize += cptLine;
            if (listViewConsole.VirtualListSize > ClassConstMessage.MaxLinesConsole - 1)
            {
                ListViewItem line = new ListViewItem("You have reached the maximum number of rows provided in the console(" + ClassConstMessage.MaxLinesConsole.ToString() + ")");
                cacheLines.Add(line);
                listViewConsole.VirtualListSize++;
                retour = true;
            }
            listViewConsole.Columns[0].Text = "Console ---nb line=" + listViewConsole.VirtualListSize.ToString() + "/" + ClassConstMessage.MaxLinesConsole.ToString();
            if (listViewConsole.VirtualListSize > 0)
            {
                ListViewItem last = listViewConsole.Items[listViewConsole.VirtualListSize - 1];
                last.EnsureVisible();
            }
            listViewConsole.EndUpdate();
            ResumeLayout(true);
            return retour;

        }
        private void ButtonSelectToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            string text = string.Empty;
            System.Windows.Forms.ListView.SelectedIndexCollection col = listViewConsole.SelectedIndices;
            if (col.Count > 0)
            {
                foreach (int item in col)
                {
                    text += listViewConsole.Items[item].Text;
                }

                Clipboard.SetText(text);
            }
        }
        private void MainTableLayoutPanel_SizeChanged(object sender, EventArgs e)
        {
            listViewConsole.Columns[0].Width = listViewConsole.Width;
        }
        private void ButtonClearMessages_Click(object sender, EventArgs e)
        {
            ClearListViewConsole();
        }
        private void ClearListViewConsole()
        {
            listViewConsole.VirtualListSize = 0;
            listViewConsole.Columns[0].Text = "Console RTL_TCP---nb line=0" + "/" + ClassConstMessage.MaxLinesConsole.ToString();
            cacheLines = null;
            listViewConsoleFull = false;
        }
        private void FreeRessources()
        {
            _owner?.Dispose();
            if (betterFolderBrowser != null)
                betterFolderBrowser = null;
            refreshTimer.Stop();
            if (refreshTimer != null)
                refreshTimer = null;
        }
        #endregion
        #region ambient property
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            UpdateGuiState();
        }
        private void RTLTcpSettings_VisibleChanged(object sender, EventArgs e)
        {
            refreshTimer.Enabled = Visible;
            UpdateGuiState();
        }
        private Color BColor = Color.Empty;
        private Color FColor = Color.Empty;
        private void UpdateGuiState()
        {
            if (_owner.GetChangeBackColourMyMainForm())
            {
                BColor = _owner.GetCurrentBackColour();
                FColor = _owner.GetCurrentForeColour();
                SetColor(this);
            }
        }
        private void SetColor(Control control)
        {
            try
            {
                if (control.Name == "comboBoxForceSampleRate")
                {
                    this.comboBoxForceSampleRate.DisabledBackColor = BColor;
                    this.comboBoxForceSampleRate.DisabledForeColor = FColor;
                    this.comboBoxForceSampleRate.BackColor = BColor;
                    this.comboBoxForceSampleRate.ForeColor = FColor;
                }
                else if (control.Name == "comboBoxForceFrequency")
                {
                    this.comboBoxForceFrequency.DisabledBackColor = BColor;
                    this.comboBoxForceFrequency.DisabledForeColor = FColor;
                    this.comboBoxForceFrequency.BackColor = BColor;
                    this.comboBoxForceFrequency.ForeColor = FColor;
                }
                else
                {
                    control.BackColor = BColor;
                    control.ForeColor = FColor;
                }
            }
            catch(Exception ex)
            {
                //Debug.WriteLine(ex.Message, "color");
            }
            //without this code pb with combo,textbox and conteneur title
            if (control.HasChildren)
            {
                foreach (Control childControl in control.Controls)
                {
                    SetColor(childControl);
                }
            }
        }
        private void ResetAllControlsBackColor(Control control)
        {
            control.BackColor = Color.Empty;
            control.ForeColor = Color.Empty;
            if (control.HasChildren)
            {
                foreach (Control childControl in control.Controls)
                {
                    ResetAllControlsBackColor(childControl);
                }
            }
        }
        #endregion

        private void RTLTcpSettings_Load(object sender, EventArgs e)
        {

        }

        private void InitColorControlForDisabled()
        {
            // 
            // buttonFolderSelect
            // 
            this.buttonFolderSelect = new ColorButton() {
                Location = new System.Drawing.Point(6, 63),
                Name = "buttonFolderSelect",
                Size = new System.Drawing.Size(67, 36),
                TabIndex = 18,
                Text = "Folder select",
                UseVisualStyleBackColor = false
            };
            this.buttonFolderSelect.Click += new System.EventHandler(this.ButtonFolderSelect_Click);

            // 
            // buttonFilesSelect
            // 
            this.buttonFilesSelect = new ColorButton()
            {
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                Location = new System.Drawing.Point(6, 19),
                Name = "buttonFilesSelect",
                Size = new System.Drawing.Size(67, 36),
                TabIndex = 18,
                Text = "Files select",
                UseVisualStyleBackColor = false
            };
            this.buttonFilesSelect.Click += new System.EventHandler(this.ButtonFilesSelect_Click);
            // 
            // groupBoxRbSr
            // 
            this.groupBoxRbSr = new ColorGroupBox()
            {
                AutoSize = true,
                Location = new System.Drawing.Point(326, 20),
                Name = "groupBoxRbSr",
                Size = new System.Drawing.Size(144, 268),
                TabIndex = 21,
                TabStop = false,
                Text = "Sample rate"
            };
            // 
            // comboBoxForceFrequency
            // 
            this.comboBoxForceFrequency = new ColorComboBoxFakeDisabled()
            {
                FormatString = "N0",
                BackColor = System.Drawing.SystemColors.Window,
                ForeColor = System.Drawing.SystemColors.WindowText,
                FormattingEnabled = true,
                Location = new System.Drawing.Point(176, 78),
                Name = "comboBoxForceFrequency",
                Size = new System.Drawing.Size(129, 21),
                DrawMode = DrawMode.OwnerDrawFixed,
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabIndex = 32
            };
            this.comboBoxForceFrequency.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBoxForceFrequency_KeyPress);
            // 
            // comboBoxForceSampleRate
            //
            this.comboBoxForceSampleRate = new ColorComboBoxFakeDisabled()
            {
                FormatString = "N0",
                FormattingEnabled = true,
                Location = new System.Drawing.Point(176, 38),
                Name = "comboBoxForceSampleRate",
                Size = new System.Drawing.Size(129, 21),
                TabIndex = 32,
                DrawMode = DrawMode.OwnerDrawFixed,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.comboBoxForceSampleRate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ComboBoxForceSampleRate_KeyPress);

            this.groupBoxTCPServer.Controls.Add(this.comboBoxForceSampleRate);
            this.groupBoxTCPServer.Controls.Add(this.comboBoxForceFrequency);
            this.groupBoxTCPServer.Controls.Add(this.buttonFolderSelect);
            this.groupBoxTCPServer.Controls.Add(this.buttonFilesSelect);
            this.groupBoxServer.Controls.Add(this.groupBoxRbSr);
        }
    }
}
