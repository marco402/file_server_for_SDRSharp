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
            //comboBoxForceFrequency.Items.Clear();

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
            string memoText = comboBoxForceFrequency.Text;
            comboBoxForceFrequency.Items.Clear();
            frequency.Sort();
            foreach (long fr in frequency)
            {
                _ = comboBoxForceFrequency.Items.Add(fr.ToString());
            }
            comboBoxForceFrequency.SelectedItem = memoText;
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
            comboBoxForceSampleRate.SetFakeEnabled(stat);
            comboBoxForceFrequency.SetFakeEnabled(stat);
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
        private void AddControlSampleRate(System.Windows.Forms.RadioButton[] RB, System.Windows.Forms.Label[] LB, Int32 indice, Int32 sr)
        {
            radioButtonsSR[indice] = new ColorRadioButton
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
            labelNbFile[indice] = new ColorLabel
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

            if (GetSampleRateForce() > 0 && !usedListSampleRate.Contains(GetSampleRateForce()))
                usedListSampleRate.Add(GetSampleRateForce());

            int nbSR = usedListSampleRate.Count();
            if (usedListSampleRate.Contains(-1))
            {
                nbSR--;
            }

            //radioButtonsSR = new System.Windows.Forms.RadioButton[nbSR];
            //  var lbl = new ColorRadioButton { Text = "Nom", DisabledForeColor = Color.Red };
            //  var btn = new ColorButton { Text = "OK", DisabledForeColor = Color.DarkBlue };
            radioButtonsSR = new ColorRadioButton[nbSR];
            labelNbFile = new ColorLabel[nbSR];
            int i = 0;
            foreach (int sr in usedListSampleRate)
            {
                if (!(sr == -1))
                {
                    AddControlSampleRate(radioButtonsSR, labelNbFile, i, sr);
                    i += 1;
                }
            }
            groupBoxRbSr.Height = (2 * _top) + (i * _space);
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

        #region color disables controls
        public class ColorButton : Button, IDisabledColorControl
        {
            public Color DisabledForeColor { get; set; } = Color.DarkGray;

            public ColorButton()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

                FlatStyle = FlatStyle.Flat;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;

                // Fond
                using (var b = new SolidBrush(BackColor))
                    g.FillRectangle(b, ClientRectangle);

                // Bord
                using (var p = new Pen(Color.FromArgb(80, 80, 80)))
                    g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);

                // Texte
                Color color = Enabled ? ForeColor : DisabledForeColor;

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    ClientRectangle,
                    color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }
        public class ColorRadioButton : RadioButton
        {
            public Color DisabledForeColor { get; set; }

            public ColorRadioButton()
            {
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);

                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(BackColor);

                bool enabled = this.IsEffectivelyEnabled();
                Color color = enabled ? ForeColor : DisabledForeColor;

                // Cercle
                int diameter = Font.Height - 2;
                int radius = diameter / 2;
                int cx = 1 + radius;
                int cy = Height / 2;

                using (var pen = new Pen(color))
                    g.DrawEllipse(pen, cx - radius, cy - radius, diameter, diameter);

                if (Checked)
                {
                    using (var b = new SolidBrush(color))
                    {
                        int inner = diameter / 2;
                        int ir = inner / 2;
                        g.FillEllipse(b, cx - ir, cy - ir, inner, inner);
                    }
                }

                // Texte
                int textOffset = diameter + 6;
                var textRect = new Rectangle(textOffset, 0, Width - textOffset, Height);

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    color,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
        public class ColorListBox : ListBox
        {
            public Color DisabledForeColor { get; set; }

            public ColorListBox()
            {
                DrawMode = DrawMode.OwnerDrawFixed;
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
                Invalidate();
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index < 0)
                    return;

                bool enabled = this.IsEffectivelyEnabled();
                Color textColor = enabled ? ForeColor : DisabledForeColor;

                // Fond
                Color back = BackColor;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && enabled)
                    back = Color.FromArgb(BackColor.R - 10, BackColor.G - 10, BackColor.B - 10);

                using (var b = new SolidBrush(back))
                    e.Graphics.FillRectangle(b, e.Bounds);

                // Texte
                TextRenderer.DrawText(
                    e.Graphics,
                    Items[e.Index].ToString(),
                    Font,
                    e.Bounds,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
        public class ColorLabel : Label
        {
            public Color DisabledForeColor { get; set; }

            public ColorLabel()
            {
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                bool enabled = this.IsEffectivelyEnabled();
                Color color = enabled ? ForeColor : DisabledForeColor;

                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    ClientRectangle,
                    color,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
        public class ColorGroupBox : GroupBox
        {
            public Color DisabledForeColor { get; set; }

            public ColorGroupBox()
            {
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
                SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnBackColorChanged(EventArgs e)
            {
                base.OnBackColorChanged(e);
                DisabledForeColor = ThemeHelper.GetDisabledColor(this.BackColor);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(BackColor);

                bool enabled = this.IsEffectivelyEnabled();
                Color textColor = enabled ? ForeColor : DisabledForeColor;

                // Mesure du texte
                Size textSize = TextRenderer.MeasureText(Text, Font);

                // Rectangle du texte (classique GroupBox)
                Rectangle textRect = new Rectangle(8, 0, textSize.Width + 2, textSize.Height);

                // Rectangle du cadre
                Rectangle borderRect = new Rectangle(
                    0,
                    textRect.Height / 2,
                    Width - 1,
                    Height - textRect.Height / 2 - 1
                );

                // Cadre
                using (var pen = new Pen(textColor))
                    g.DrawRectangle(pen, borderRect);

                // Effacer le fond derrière le texte pour éviter la ligne qui passe dedans
                using (var b = new SolidBrush(BackColor))
                    g.FillRectangle(b, textRect);

                // Texte
                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }
        }
        public static class ThemeHelper
        {
            public static Color GetDisabledColor(Color background)
            {
                // Calcul de luminance perceptuelle
                double luminance =
                    0.2126 * background.R +
                    0.7152 * background.G +
                    0.0722 * background.B;

                // Si fond sombre → texte disabled clair
                if (luminance < 128)
                    return Color.FromArgb(180, 180, 180);

                // Si fond clair → texte disabled foncé
                return Color.FromArgb(100, 100, 100);
            }
        }
    }
    public static class ControlExtensions
    {
        public static bool IsEffectivelyEnabled(this Control c)
        {
            while (c != null)
            {
                if (!c.Enabled)
                    return false;
                c = c.Parent;
            }
            return true;
        }
    }
    #endregion
}
