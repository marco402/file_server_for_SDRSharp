/* Written by Marc Prieur (marco40_github@sfr.fr)
                                   FormSendFilesFct.cs 
                                   project RTLTCP
 **************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license: 
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.

 model with https://github.com/sobieh/sdrsharp_rtl_tcp_direct/tree/master
  **********************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SDRSharp.RTLTCP
{
    public partial class RTLTcpSettings : Form
    {
        #region internal functions
        internal Dictionary<string, InfoFile> GetListFiles(int _sampleRate)
        {
            int indiceSR = 0;
            foreach (int sr in usedListSampleRate)
            {
                if (sr == _sampleRate)
                {
                    break;
                }

                indiceSR++;
            }
            return listOfListFiles[indiceSR];
        }
        internal int NEmissionForEachFile => int.Parse(textBoxNEmissionForEachFile.Text, CultureInfo.InvariantCulture);
        internal int NEmissionForAllFiles => int.Parse(textBoxNEmissionForAllFiles.Text, CultureInfo.InvariantCulture);
        internal int TempoBetweenFile => int.Parse(textBoxTempoBetweenFile.Text, CultureInfo.InvariantCulture);
        internal void Start()
        {
            EnabledDisableControls(false);
            buttonStartRadio.Text = ClassConstMessage.STOPRADIO;
        }
        internal void Stop()
        {
            EnabledDisableControls(true);
            buttonStartRadio.Text = ClassConstMessage.STARTRADIO;
        }
        internal void StopRd()
        {
            _owner.StopRadio();
        }
        internal bool TestsOnStart()
        {
            if (Files == null)
            {
                _ = MessageBox.Show("Choose a folder or files and after choose a sample rate", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (_owner.Samplerate == -2)
            {
                _ = MessageBox.Show("Choose a sample rate", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (comboBoxForceFrequency.Text.Trim() == string.Empty)
            {
                _owner.FrequencyForce = -1;
            }
            else
            {
                int fr = int.Parse(comboBoxForceFrequency.Text, CultureInfo.InvariantCulture);
                if (fr > 0 || fr <= _owner.MaximumTunableFrequency)  //_owner.MinimumTunableFrequency
                {
                    _owner.FrequencyForce = int.Parse(comboBoxForceFrequency.Text, CultureInfo.InvariantCulture);
                }
                else
                {
                    _ = MessageBox.Show($"forced frequency beyond limit{1000000} to {_owner.MaximumTunableFrequency}", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            if (comboBoxForceSampleRate.Text.Trim() == string.Empty)
            {
                _owner.SampleRateForce = -1;
            }
            else
            {
                int sr = int.Parse(comboBoxForceSampleRate.Text, CultureInfo.InvariantCulture);
                if (sr > 0 || sr <= sampleRate[sampleRate.Count - 1])
                {
                    _owner.SampleRateForce = int.Parse(comboBoxForceSampleRate.Text, CultureInfo.InvariantCulture);
                }
                else
                {
                    _ = MessageBox.Show($"forced sample rate beyond limit 0  to {_owner.MaximumTunableFrequency}", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            SaveSetting();
            return true;
        }
        #endregion
        #region private functions 
        /// <summary>
        /// Sorting of selected IQ or Wav files
        /// Frequence:
        ///     If the frequency is not found in the file name, 2 cases:
        ///         -The comboBoxForceFrequency displays a frequency, it will be used.
        ///         -Otherwise, the file is not processed.
        /// Sample rate:
        ///      If the sample rate is not found in the file name, 2 cases:
        ///         -The comboBoxForceSampleRate displays a sample rate, it is added to the list of found sample rates.
        ///         -Otherwise, the file is not processed.
        /// </summary>
        /// <param name="listListFiles">list of files collections, 1 collection for each sample rate +
        /// 1 collection for sample rate not in the name
        /// Reject only if Length=0
        /// Sample rate ="-1" if not in the name
        /// Frequency = 0  if not in the name</param>
        /// <param name="Files">All selected files</param>
        /// <param name="usedListSampleRate">List of Sample rate + Sample rate ="-1" if not in the name</param>
        /// <returns></returns>
        private void TriSampleRate(List<Dictionary<string, InfoFile>> listListFiles, string[] Files)
        {
            int sampleRate = 0;
            int sampleRateForceInt = GetSampleRateForce();
            long frequencyForce = GetfrequencyForce();
            listFrequency.Clear();
            comboBoxForceFrequency.Items.Clear();

            foreach (string file in Files)
            {
                InfoFile d = new InfoFile();
                FileInfo f = new FileInfo(file);
                if (f.Length == 0)
                {
                    AddMessage($"Size=0 for {file}\n");
                    continue;
                }
                d.Frequency = ProcessFrequency(file, ref listFrequency);
                if (d.Frequency == -1 && frequencyForce == 0)
                    cpFilesWithoutFrequency++;
                string retSampleRate = ProcessSampleRate(file, ref sampleRate, sampleRateForceInt, d.Frequency, frequencyForce);
                if (retSampleRate == "-2")
                    continue;
                d.SampleRate = retSampleRate;
                int indiceSR = GetIndiceSampleRateToListListFiles(sampleRate);
                listListFiles[indiceSR].Add(file, d);
            }

            DisplayFrequency(listFrequency);

            return;
        }
        private int GetIndiceSampleRateToListListFiles(int sampleRate)
        {
            int indiceSR = 0;
            //int srTest = sampleRate;
            //if (sampleRate == -1 && sampleRateForce == string.Empty)
            //{
            //    srTest = -1;
            //}

            foreach (int sr in usedListSampleRate)
            {
                if (sampleRate == sr)   //srTest
                {
                    break;
                }
                indiceSR++;
            }
            return indiceSR;
        }
        /// <summary>
        /// process sample rate 
        /// return sample rate or -2 and add a list of sample rate in usedListSampleRate
        /// forced
        /// name file with sample rate             no forced sample rate         forced sample rate
        ///     yes                                 ok sample rate               /
        ///     no                                  ignored file                 ok forced sample rate            
        /// </summary>
        /// <param name="file"></param>
        /// <param name="sampleRate"></param>
        /// <param name="sampleRateForceInt"></param>
        /// <param name="Frequency"></param>
        /// <param name="frequencyForce"></param>
        /// <returns>sample rate if ok else -2</returns>
        private string ProcessSampleRate(string file, ref int sampleRate, int sampleRateForceInt, long Frequency, long frequencyForce)
        {
            ulong SampleRate64 = 0;
            if (Path.GetExtension(file).ToLower() == ".wav")
            {
                string message = string.Empty;
                if (!ClassWav.GetSampleRate(file, ref sampleRate, ref SampleRate64, ref message))
                {
                    AddMessage($"{message} for {file}\n");
                    return "-2";
                }
            }
            else
                 sampleRate = WavRecorder.GetSampleRateFromName(file);
 

            if (sampleRateForceInt == 0 && sampleRate == -1)   //case ignored file
                return "-2";
            else if (sampleRateForceInt > 0 && sampleRate == -1)
            {
                sampleRate = sampleRateForceInt;    //case forced sample rate
            }

            if (!usedListSampleRate.Contains(sampleRate))
            {
                usedListSampleRate.Add(sampleRate);

                Dictionary<string, InfoFile> newSr = new Dictionary<string, InfoFile>();
                listOfListFiles.Add(newSr);
            }
            return sampleRate.ToString();
        }
        private long ProcessFrequency(string file, ref List<long> frequency)
        {
            string F = GetFrequencyFromName(file);
            long FLong = 0;
            if (F != string.Empty)
            {
                FLong = GetFrequencyFromString(F);   //from 433.92m return 433920000
                if (!frequency.Contains(FLong))
                {
                    frequency.Add(FLong);
                }
            }
            if (FLong <= 0)
            {
                FLong = -1;
            }

            return FLong;
        }
        private void DisplayFrequency(List<long> frequency)
        {
            frequency.Sort();
            foreach (long fr in frequency)
            {
                _ = comboBoxForceFrequency.Items.Add(fr.ToString());
            }
        }
        private bool ProcessUsedListSampleRate(int sampleRate, int sampleRateForceInt)
        {
            if (!usedListSampleRate.Contains(sampleRate) && sampleRate > 0)
            {
                usedListSampleRate.Add(sampleRate);
                return true;
            }
            else if (sampleRate == -1 && sampleRateForce != string.Empty && !usedListSampleRate.Contains(sampleRateForceInt))
            {
                usedListSampleRate.Add(int.Parse(sampleRateForce, CultureInfo.InvariantCulture));
                return true;
            }
            return false;
        }
        private long GetFrequencyFromString(string Frequency)
        {
            if (Frequency == string.Empty)
            {
                return -1;
            }

            Frequency = Frequency.ToLower();
            string[] units = { "khz", "hz", "k", "m", "" };
            int[] coef = { 1000, 1, 1000, 1000000, 1 };
            int indice = 0;
            foreach (string unit in units)
            {
                if (Frequency.Contains(unit))
                {
                    try
                    {
                        float freqInt = float.Parse(Frequency.Substring(0, Frequency.Length - unit.Length), CultureInfo.InvariantCulture);
                        freqInt *= coef[indice];
                        if (freqInt <= 0 || freqInt > _owner.MaximumTunableFrequency)
                        {
                            return -1;
                        }
                        else
                        {
                            decimal d = Convert.ToDecimal(freqInt);
                            return Convert.ToInt64(d);
                        }
                    }
                    catch
                    {
                        return -1;
                    }
                }
                indice += 1;
            }
            return -1;
        }
        //dup with WavRecorder
        private string GetFrequencyFromName(string fileName)
        {
            string frequencyStr;
            fileName = Path.GetFileName(fileName);
            frequencyStr = GetStringFrequency(fileName); //return 433.92m
            if (frequencyStr != string.Empty)
            {
                return frequencyStr;
            }
            //}
            //}
            return string.Empty;
        }
        //dup with WavRecorder
        private string GetStringFrequency(string fileName)
        {
            fileName = fileName.ToLower();
            string retString = string.Empty;
            FileInfo Fi = new FileInfo(fileName);
            if (Fi.Extension == ".wav")
            {
                fileName = fileName.Replace(Fi.Extension, "");
                // format 'Baseband : Simple recorder' ->08-42-37_433920000Hz.wav
                string[] cutLine = fileName.Split('_');
                for (int i = 0; i < cutLine.Length; i++)
                {
                    if (cutLine[i].Contains("hz"))
                    {
                        retString = cutLine[i].Replace("hz", "");
                        try
                        {
                            float resultat = float.Parse(retString, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            return string.Empty;
                        }
                        return retString;
                    }
                }
            }
            else
            {
                string[] units = { "hz", "k", "khz", "m" };   //, "Hz", "K", "Khz", "M"
                int lastCar = 0;
                int fin = fileName.Length - 3;

                int cptUnderscore = 0;
                for (int i = fin; i > 0; i--)
                {
                    if (fileName.Substring(i, 1) == "_" && cptUnderscore == 0)
                    {
                        lastCar = i - 1;
                        cptUnderscore += 1;
                    }
                    else if (fileName.Substring(i, 1) == "_" && cptUnderscore == 1)
                    {
                        int startCar = i + 1;
                        string foundUnit = string.Empty;
                        bool okUnit = false;
                        foreach (string unit in units)
                        {
                            foundUnit = fileName.Substring(startCar + lastCar - startCar - unit.Length + 1, unit.Length);
                            if (foundUnit == unit)
                            {
                                okUnit = true;
                                break;
                            }
                        }
                        if (okUnit)
                        {
                            retString = fileName.Substring(startCar, lastCar - startCar - foundUnit.Length + 1);
                            try
                            {
                                float resultat = float.Parse(retString, CultureInfo.InvariantCulture);
                                return okUnit ? retString + foundUnit : fileName.Substring(startCar, lastCar - startCar + foundUnit.Length);
                            }
                            catch
                            {
                                return string.Empty;
                            }
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
            }
            return retString;
        }
        private bool GetFiles(ref string[] Files, string ext)
        {
            using (OpenFileDialog openFiles = new OpenFileDialog())
            {
                openFiles.DefaultExt = ext;
                openFiles.Filter = ext + " files|*." + ext;
                openFiles.Multiselect = true;
                if (openFiles.ShowDialog() == DialogResult.OK)
                {
                    Files = openFiles.FileNames;
                    return true;
                }
                return false;
            }
        }
        private void EnabledDisableControls(bool stat)
        {
            textBoxNEmissionForAllFiles.Enabled = stat;
            textBoxNEmissionForEachFile.Enabled = stat;
            textBoxTempoBetweenFile.Enabled = stat;
            comboBoxForceSampleRate.Enabled = stat;
            comboBoxForceFrequency.Enabled = stat;
            buttonFilesSelect.Enabled = stat;
            buttonFolderSelect.Enabled = stat;
            groupBoxRbSr.Enabled = stat;
        }
        private void AddMessage(string message)
        {
            if (message.Contains(ClassConstMessage.RESTLOOPFILE))
            {
                labelNbSendingForEachFile.Text = message;
                Refresh();
                return;
            }
            if (message.Contains(ClassConstMessage.NUMFILE))
            {
                labelNumFile.Text = labelNumFile.Text = message + "/" + labelNbFile[indiceNbFiles].Text;
                Refresh();
                return;
            }
            if (message.Contains(ClassConstMessage.RESTLOOPALLFILE))
            {
                labelNbSendingForAllFiles.Text = message;
                Refresh();
                return;
            }
            else if (message == ClassConstMessage.STARTRADIO)
            {
                buttonStartRadio.Text = ClassConstMessage.STOPRADIO;
                buttonStartRadio.BackColor = Color.Green;
                Refresh();
                return;
            }
            else if (message == ClassConstMessage.STOPRADIO || message == ClassConstMessage.STOPPED || message.Contains(ClassConstMessage.ABANDON) || message == ClassConstMessage.COMPLETED)
            {
                buttonStartRadio.Text = ClassConstMessage.STARTRADIO;
                buttonStartRadio.BackColor = SystemColors.ControlDarkDark;
            }
            if (!listViewConsoleFull)
            {
                listViewConsoleFull = WriteLine(message);
            }

            Application.DoEvents();
            Refresh();
            return;
        }
        private void ClearSampleRateUsedList()
        {
            if (radioButtonsSR != null)
            {
                foreach (RadioButton ctrl in radioButtonsSR)
                {
                    ctrl.Dispose();
                    Controls.Remove(ctrl);
                }
            radioButtonsSR = null;
            }
            if (labelNbFile != null)
            {
                foreach (Label ctrl in labelNbFile)
                {
                    ctrl.Dispose();
                    Controls.Remove(ctrl);
                }
                labelNbFile = null;
            }
         }
        private const int _leftRD = 10;
        private const int _leftLB = 80;
        private const int _top = 15;
        private const int _space = 20;
        private void AddControlSampleRate(System.Windows.Forms.RadioButton[] RB, System.Windows.Forms.Label[] LB, Int32 indice,Int32 sr)
        {
            radioButtonsSR[indice] = new RadioButton
            {
                Text = sr.ToString(),
                Location = new System.Drawing.Point(_leftRD, _top + (indice * _space)),
                ForeColor = labelSRForce.ForeColor,
                BackColor = labelSRForce.BackColor,
                UseVisualStyleBackColor = true,
                Size = new System.Drawing.Size(70, 17),
                Tag = sr,
                Enabled = true
            };
            radioButtonsSR[indice].CheckedChanged += new System.EventHandler(RadioButtonsSR_CheckedChanged);
            groupBoxRbSr.Controls.Add(radioButtonsSR[indice]);
            labelNbFile[indice] = new Label
            {
                BackColor = labelSRForce.BackColor,
                ForeColor = labelSRForce.ForeColor,
                Location = new System.Drawing.Point(_leftLB, _top + (indice * _space)),
                Name = "labelNbFile",
                Size = new System.Drawing.Size(60, 13),
                Text = "0"
            };
            groupBoxRbSr.Controls.Add(labelNbFile[indice]);

        }
        private void DisplaySampleRateUsedList(List<int> usedListSampleRate)
        {
            ClearSampleRateUsedList();

            if (GetSampleRateForce()>0 && !usedListSampleRate.Contains(GetSampleRateForce())) 
                usedListSampleRate.Add(GetSampleRateForce());

            int nbSR = usedListSampleRate.Count();
            if (usedListSampleRate.Contains(-1))
            {
                nbSR--;
            }
            
            radioButtonsSR = new System.Windows.Forms.RadioButton[nbSR];
            labelNbFile = new System.Windows.Forms.Label[nbSR];
            int i = 0;
            foreach (int sr in usedListSampleRate)
            {
                if (!(sr == -1))
                {
                    AddControlSampleRate(radioButtonsSR, labelNbFile, i, sr);
                    i += 1;
                }
            }
            groupBoxRbSr.Height =  (2 * _top) + (i * _space);
            //groupBoxRbSr.Size = new System.Drawing.Size(482, (2 * _top) + (i * _space));
        }
        private int GetSampleRateForce()
        {
            sampleRateForce = comboBoxForceSampleRate.Text.Trim();
            int sampleRateForceInt = 0;
            if (comboBoxForceSampleRate.Text.Trim() != string.Empty)
            {
                _ = int.TryParse(sampleRateForce, out sampleRateForceInt);
            }

            return sampleRateForceInt;
        }
        private long GetfrequencyForce()
        {
            frequencyForce = comboBoxForceFrequency.Text.Trim();
            long frequencyForceLng = 0;
            if (comboBoxForceFrequency.Text.Trim() != string.Empty)
            {
                _ = long.TryParse(frequencyForce, out frequencyForceLng);
            }

            return frequencyForceLng;
        }
        #endregion
        internal class InfoFile
        {
            public long Frequency { get; set; } = -1;
            public string SampleRate { get; set; } = string.Empty;
        }
    }
}
