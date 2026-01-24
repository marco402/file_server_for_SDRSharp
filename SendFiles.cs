/* Written by Marc Prieur (marco40_github@sfr.fr)
                                 SendFiles.cs 
                                project RTLTCP
 **************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license: 
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.

 model with https://github.com/sobieh/sdrsharp_rtl_tcp_direct/tree/master
  **********************************************************************************/

using SDRSharp.Radio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TestRF64;

namespace SDRSharp.RTLTCP
{
    public unsafe class RtlTcpIO : IFrontendController, IDisposable, IIQStreamController, ITunableSource, IFloatingConfigDialogProvider
    {
        #region declare
        private static readonly float* m_lutPtr;
        private static readonly UnsafeBuffer m_lutBuffer = UnsafeBuffer.Create(256, sizeof(float));
        private SamplesAvailableDelegate m_callback;
        private Thread m_sampleThread;
        private UnsafeBuffer m_iqBuffer;
        private Complex* m_iqBufferPtr;
        private readonly byte[] m_cmdBuffer = new byte[5];
        private readonly RTLTcpSettings m_gui;
        private IWin32Window parent;
        private MainForm myMainForm;
        private bool m_CanTune = false;
        private readonly int _TempoBetweenFile = 0;
        private Dictionary<string, RTLTcpSettings.InfoFile> listFiles;
        internal event EventHandler<string> MessageReceived;
        #endregion
        #region Public functions
        internal void StartRadio()
        {
            myMainForm.StartRadio();
        }
        internal void StopRadio()
        {
            myMainForm.StopRadio();
        }
        static RtlTcpIO()
        {
            m_lutPtr = (float*)m_lutBuffer;

            const float scale = 1.0f / 127.5f;
            for (int i = 0; i < 256; i++)
            {
                m_lutPtr[i] = (i - 127.5f) * scale;
            }
        }
        public RtlTcpIO()
        {
            m_gui = new RTLTcpSettings(this);
        }
        ~RtlTcpIO()
        {
            Dispose();
        }
        private int _NEmissionForAllFiles = 0;
        internal int NEmissionForAllFiles
        {
            set => _NEmissionForAllFiles = value;
        }
        private int _NEmissionForEachFile = 0;
        internal int NEmissionForEachFile
        {
            set => _NEmissionForEachFile = value;
        }
        private int _tempoBetweenFile;
        internal int TempoBetweenFile
        {
            set => _tempoBetweenFile = value;
        }
        private int _frequencyForce;
        internal int FrequencyForce
        {
            set => _frequencyForce = value;
        }
        private int _sampleRateForce;
        internal int SampleRateForce
        {
            set => _sampleRateForce = value;
        }
        #endregion
        #region Event
        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }
        #endregion
        #region Worker Thread
        private bool run = false;
        internal const int NBBYTEFORRTL_433 = 131072;
        private void ReceiveSamples()
        {
            m_iqBuffer = UnsafeBuffer.Create(NBBYTEFORRTL_433, sizeof(Complex));
            m_iqBufferPtr = (Complex*)m_iqBuffer;
            ForBreak();

            m_sampleThread = null;
            m_callback = null;
            if (m_iqBuffer != null)
            {
                m_iqBuffer.Dispose();
                m_iqBufferPtr = null;
                StopRadio();
            }
            if (listFiles != null)
            {
                listFiles = null;
            }
        }
        private void ForBreak()
        {
            byte[] byteSend = null;
#if !RF64
            float[] L = null;
            int size = 0;
            int sampleRateFile = 0;
#endif

            string message = string.Empty;
            string messageRaw = string.Empty;
            while (run)
            {
                int[] NFile = new int[_NEmissionForAllFiles];
                for (int EmissionForAllFiles = 0; EmissionForAllFiles < _NEmissionForAllFiles; EmissionForAllFiles++)
                {
                    OnMessageReceived($"{ClassConstMessage.RESTLOOPALLFILE}{EmissionForAllFiles + 1}/{_NEmissionForAllFiles}");
                    foreach (KeyValuePair<string, RTLTcpSettings.InfoFile> file in listFiles)
                    {
                        bool OK = true;

                        if (Path.GetExtension(file.Key).ToLower() == ".wav")
                        {
#if !RF64
                            if (!ClassWav.ReadWav(file.Key, ref byteSend, ref L, ref size, false, ref message, ref sampleRateFile))
                            {
                                OnMessageReceived($"{message} for {file.Key}\n");
                                continue;
                            }
                            myMainForm.Frequency = file.Value.Frequency == -1 ? _frequencyForce : file.Value.Frequency;

                            NFile[EmissionForAllFiles] += 1;
                            OnMessageReceived($"{ClassConstMessage.NUMFILE}{NFile[EmissionForAllFiles]}");
                            byteSend = new byte[L.Length];
                            _ = ClassUtils.ConvertFloatToByte(L, byteSend);
#else
                            //for using add <LangVersion>8.0</LangVersion> to csproj

                            using var reader = new Rf64StreamingReader(file.Key);
                            {
                                myMainForm.Frequency = file.Value.Frequency == -1 ? _frequencyForce : file.Value.Frequency;
                                NFile[EmissionForAllFiles] += 1;
                                OnMessageReceived($"{ClassConstMessage.NUMFILE}{NFile[EmissionForAllFiles]}");
                                for (int EmissionForEachFile = 0; EmissionForEachFile < _NEmissionForEachFile; EmissionForEachFile++)
                                {
                                    if (!run)
                                    {
                                        return;
                                    }
                                    if (OK)
                                    {
                                        if (EmissionForAllFiles == 0 && EmissionForEachFile == 0)
                                        {
                                            OnMessageReceived($"{myMainForm.Frequency}Hz for  {Path.GetFileName(file.Key)}\n");
                                        }

                                        OnMessageReceived($"{ClassConstMessage.RESTLOOPFILE}{EmissionForEachFile + 1}/{_NEmissionForEachFile}");
              //part wav
                                        byte[] buffer = new byte[1024 * 1024]; // 1 MB
                                        int read;

                                        while ((read = reader.ReadSamples(buffer, 0, buffer.Length)) > 0)
                                        {
                                                fixed (byte* ptr = buffer)
                                                {
                                                    ProcessSamples(ptr, read);
                                                }
                                         }
            //end wav
                                        Thread.Sleep(_TempoBetweenFile);
                                    }
                                }
                            }
#endif
                        }
                        else
                        {
                            myMainForm.Frequency = file.Value.Frequency == -1 ? _frequencyForce : file.Value.Frequency;

                            NFile[EmissionForAllFiles] += 1;
                            OnMessageReceived($"{ClassConstMessage.NUMFILE}{NFile[EmissionForAllFiles]}");
//part CU8                           
                            byteSend = ClassUtils.GetDataFile(file.Key, ref messageRaw);
//end CU8

#if !RF64
                            }
#endif
                            for (int EmissionForEachFile = 0; EmissionForEachFile < _NEmissionForEachFile; EmissionForEachFile++)
                            {
                                if (!run)
                                {
                                    return;
                                }

                                if (OK)
                                {
                                    if (EmissionForAllFiles == 0 && EmissionForEachFile == 0)
                                    {
                                        OnMessageReceived($"{myMainForm.Frequency}Hz for  {Path.GetFileName(file.Key)}\n");
                                    }

                                    OnMessageReceived($"{ClassConstMessage.RESTLOOPFILE}{EmissionForEachFile + 1}/{_NEmissionForEachFile}");
//part CU8                                    
                                    fixed (byte* pointerDataSend = &byteSend[0])
                                    {
                                        ProcessSamples(pointerDataSend, byteSend.Length);
                                    }
//end cu8   
                                 Thread.Sleep(_TempoBetweenFile);
                                }
#if RF64
                            }
#endif
                        }
                    }
                }
                return;
            }
            return;
        }
        private void ProcessSamples(byte* rawPtr, int lenFile)
        {
            int sampleComplexCount = lenFile / 2;
            int i = 0;
            Complex* ptr = m_iqBufferPtr;
            int j = 0;
            int memj = 0;
            for (i = 0; i < sampleComplexCount; i++)
            {
                ptr->Real = m_lutPtr[*rawPtr++];
                ptr->Imag = m_lutPtr[*rawPtr++];
                ptr++;
                j++;
                if ((j - memj) == NBBYTEFORRTL_433)
                {
                    m_callback?.Invoke(this, m_iqBufferPtr, NBBYTEFORRTL_433);
                    memj = j;
                    ptr = m_iqBufferPtr;
                }
            }
            if ((j - memj) > 0)
            {
                m_callback?.Invoke(this, m_iqBufferPtr, j - memj);
            }
        }
#endregion
        #region ambient property
        //ambient property does't OK ?? ok for plugin RTL_433 ?
        private Color memoBackColour = Color.Red;
        private Color memoForeColour = Color.Empty;
        private Color currentBackColour = Color.Empty;
        private Color currentForeColour = Color.Empty;
        public System.Drawing.Color GetCurrentBackColour()
        {
            return memoBackColour;
        }
        public Color GetCurrentForeColour()
        {
            return memoForeColour;
        }
        private bool end = false;
        private void Test(Control control)
        {
            if (end)
            {
                return;
            }

            if (control.Name.Contains("label") && control.BackColor != Color.Transparent)
            {
                end = true;
                currentBackColour = control.BackColor;
                currentForeColour = control.ForeColor;
                return;
            }
            if (control.HasChildren)
            {
                foreach (Control childControl in control.Controls)
                {
                    Test(childControl);
                }
            }
        }
        internal bool GetChangeBackColourMyMainForm()
        {
            end = false;
            Test(myMainForm);
            if (currentBackColour != memoBackColour && currentForeColour != memoForeColour)
            {
                memoBackColour = currentBackColour;
                memoForeColour = currentForeColour;
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region interfaces
        #region interface IFloatingConfigDialogProvider
        public void ShowSettingGUI(IWin32Window _parent)
        {
            parent = _parent;
            IntPtr H = parent.Handle;
            myMainForm = H == IntPtr.Zero ? null : Control.FromHandle(H) as MainForm;
            m_gui.Show();
        }
        public void HideSettingGUI()
        {
            m_gui.Hide();
        }
        #endregion
        #region IIQStreamController
        public double Samplerate { get; set; } = -2;
        #endregion
        #region ITunableSource
        public bool CanTune
        {
            get => true; set => m_CanTune = CanTune;
        }
        public long MinimumTunableFrequency => 0L;
        public long MaximumTunableFrequency => 2500000000L;
        public long Frequency { get; set; }
        #endregion
        #region IFrontendController
        public void Open()
        {
        }
        public void Close()
        {
        }
        #endregion
        #region IIQStreamController
        public void Start(SamplesAvailableDelegate callback)
        {
            if (!m_gui.TestsOnStart())
            {
                return;
            }
            //sample rate read before and  after start by SDRSharp
            NEmissionForEachFile = m_gui.NEmissionForEachFile;
            NEmissionForAllFiles = m_gui.NEmissionForAllFiles;
            TempoBetweenFile = m_gui.TempoBetweenFile;

            listFiles = m_gui.GetListFiles((int)Samplerate);

            m_gui.Start();
            m_callback = callback;
            run = true;
            m_sampleThread = new Thread(ReceiveSamples)
            {
                Name = "sendFiles"
            };
            m_sampleThread.Start();
        }
        public void Stop()  //call before radio start
        {
            run = false;
            m_gui.Stop();
        }
        #endregion
        #region IDisposable
        public void Dispose()  //no call by SDRSharp call by SDRSharp->formSendFile
        {
            if (m_iqBuffer != null)
            {
                m_iqBuffer.Dispose();
                m_iqBufferPtr = null;
            }
            GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}
