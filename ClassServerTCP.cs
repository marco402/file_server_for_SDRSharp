/* Written by Marc Prieur (marco40_github@sfr.fr)
                                   ClassServerTCP.cs 
                               project ServerForSDRSharp
 **************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license: 
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.
  **********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server_for_SDRSharp
{
    class ClassServerTCP: IDisposable
    {
         //string[] _Files;
        internal Dictionary<string, string> _listFiles;
         Boolean EndReceptTCP = false;
         private TcpListener Server;
         private TcpClient client;
         public event EventHandler<string> MessageReceived;
         NetworkStream stream;

        public async Task Start(int sampleRat)
        {
                try
                {
                if (Server != null)
                {
                    EndReceptTCP = true;
                    if (client != null)
                    {
                        OnMessageReceived($"Connected! Client IP: {client.Client.RemoteEndPoint}");
                        _ = HandleClientAsync(client, sampleRat);
                    }
                 }
                else
                {
                    IPAddress localhost = IPAddress.Parse(FormServerForSDRSharp.IPAdress);

                    Server = new TcpListener(localhost, _PortTCP);
                    Server.Start(1000);

                    OnMessageReceived("Server started. Waiting for a connection...If radio SDRSharp already started:restart radio SDRSharp");
                    EndReceptTCP = true;
                    while (EndReceptTCP)
                    {
                        client = await Server.AcceptTcpClientAsync();
                        OnMessageReceived($"Connected! Client IP: {client.Client.RemoteEndPoint}");
                        _ = HandleClientAsync(client, sampleRat);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.HResult != -2147467259)
                {
                    OnMessageReceived($"Port already in use, change port {ClassConstMessage.ABANDON}");
                }
                else if(e.HResult != -2146232798)
                    OnMessageReceived(e.Message);

            }
        }
        int memoSampleRate = -1;
        private async Task HandleClientAsync(TcpClient client, int UsedSampleRate)
        {
            stream = client.GetStream();
            Int32 NFile = 0;
            //Int32 UsedSampleRate = 0;
            if (UsedSampleRate != 0 && memoSampleRate!=UsedSampleRate)
            {
                MessageBox.Show($"Select sample on SDRSharp:{UsedSampleRate}","Choose sample rate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OnMessageReceived($"{ClassConstMessage.SAMPLERATE} {UsedSampleRate}");
                memoSampleRate = UsedSampleRate;
            }
            while (EndReceptTCP)
            {
                for (Int32 EmissionForAllFiles = 0; EmissionForAllFiles < _NEmissionForAllFiles; EmissionForAllFiles++)
                {
                    OnMessageReceived($"{ClassConstMessage.RESTLOOPALLFILE}{EmissionForAllFiles+1}/{_NEmissionForAllFiles}");
                    NFile = 0;
                    foreach (KeyValuePair<string, string> file in _listFiles)
                    {
                        float[] L = null;
                        float[] R = null;
                        byte[] byteArray = null;
                        Int32 size = 0;
                        String message = "";
                        byte[] byteSend = null;
                        Int32 sampleRate = 0;

                        if (Path.GetExtension(file.Key) == ".wav")
                        {
                            if (!ClassWav.readWav(file.Key, ref byteArray, ref R, ref L, ref size, false, ref message, ref sampleRate))
                            {
                                OnMessageReceived($"{message} for {file.Key}");
                                continue;
                            }
                            int sr = 0;
                            Int32.TryParse(file.Value, out sr);
                            if (UsedSampleRate != sr)    // sampleRate && sampleRate !=-1)  //for raw if not sample rate from name keep file
                            {
                                OnMessageReceived($"different sample rate for {file.Key}");
                                continue;
                            }
                            //else
                            //{
                            //    if (UsedSampleRate == 0)
                            //    {
                            //        UsedSampleRate = sampleRate;
                            //        OnMessageReceived($"{ClassConstMessage.SAMPLERATE} {UsedSampleRate}");
                            //    }
                            //}
                            if (UsedSampleRate != sampleRate)
                            {
                                //OnMessageReceived($"different sample rate for {file.Key}");
                                continue;
                            }
                            NFile += 1;
                            OnMessageReceived($"{ClassConstMessage.NUMFILE}{NFile}/{_listFiles.Count()}");
                            OnMessageReceived(file.Key);
                            byteSend = new byte[L.Length];
                            ClassUtils.ConvertFloatToByte(L, byteSend);
                            for (Int32 EmissionForEachFile = 0; EmissionForEachFile < _NEmissionForEachFile; EmissionForEachFile++)
                            {
                                OnMessageReceived($"{ClassConstMessage.RESTLOOPFILE}{EmissionForEachFile+1}/{_NEmissionForEachFile}");
                                try
                                {
                                    await stream.WriteAsync(byteSend, 0, byteSend.Length);
                                }
                                catch (SocketException e)
                                {
                                    if (e.HResult == -2146232800)
                                    {
                                        OnMessageReceived($"{e.Message} {ClassConstMessage.ABANDON}");
                                        return;
                                    }
                                    else
                                        OnMessageReceived(e.Message);
                                }
                                Thread.Sleep(_tempoBetweenFile);
                                if (!EndReceptTCP)
                                {
                                    OnMessageReceived(ClassConstMessage.STOPSERVER);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            String messageRaw = "";
                            byteSend = ClassUtils.getDataFile(file.Key, ref messageRaw);
                            if (byteSend==null || byteSend.Length == 0||messageRaw!="")
                            {
                                OnMessageReceived($"{file.Key} : {messageRaw} ");
                                continue;
                            }
                            // sampleRate = WavRecorder.GetSampleRateFromName(file.Key);
                            //{
                            //    if (UsedSampleRate == 0 && sampleRate!=-1)
                            //    {
                            //        UsedSampleRate = sampleRate;
                            //        OnMessageReceived($"{ClassConstMessage.SAMPLERATE} {UsedSampleRate}");
                            //    }
                            //}
                            int sr = 0; 
                            Int32.TryParse(file.Value,out sr);
                            if (UsedSampleRate != sr)    // sampleRate && sampleRate !=-1)  //for raw if not sample rate from name keep file
                            {
                                OnMessageReceived($"different sample rate for {file.Key}");
                                continue;
                            }
                            NFile += 1;
                            OnMessageReceived($"{ClassConstMessage.NUMFILE}{NFile}/{_listFiles.Count()}");
                            OnMessageReceived(file.Key);
                            for (Int32 EmissionForEachFile = 0; EmissionForEachFile < _NEmissionForEachFile; EmissionForEachFile++)
                            {
                                OnMessageReceived($"{ClassConstMessage.RESTLOOPFILE}{EmissionForEachFile+1}/{_NEmissionForEachFile}");
                                try
                                {
                                    await stream.WriteAsync(byteSend, 0, byteSend.Length);
                                }
                                catch (SocketException e)
                                {
                                    if (e.HResult == -2146232800)
                                    {
                                        OnMessageReceived($"{e.Message}  {ClassConstMessage.ABANDON}");
                                        return;
                                    }
                                    else
                                        OnMessageReceived(e.Message);
                                }
                                Thread.Sleep(_tempoBetweenFile);
                                if (!EndReceptTCP)
                                {
                                    OnMessageReceived(ClassConstMessage.STOPPED);
                                    return;
                                }
                            }
                        }
                    }
                }
                OnMessageReceived(ClassConstMessage.COMPLETED);
                break;
            }     
        }
        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        internal void Stop()
        {
            EndReceptTCP = false;
            //Server = null;
        }

        public void Dispose()
        {
            if(client != null)
            {
                client.Close();
               ((IDisposable)client).Dispose();
            }
            if(stream!=null)
            {
                stream.Close();
               ( (IDisposable)stream).Dispose();
            }
        }

        private Int32 _PortTCP;
        private Int32 _tempoBetweenFile;
        private Int32 _NEmissionForAllFiles;
        private Int32 _NEmissionForEachFile;
        internal Int32 NEmissionForAllFiles
        {
            set
            {
                _NEmissionForAllFiles = value;
            }
        }
        internal Int32 NEmissionForEachFile
        {
            set
            {
                _NEmissionForEachFile = value;
            }
        }
        internal Int32 tempoBetweenFile
        {
            set
            {
                _tempoBetweenFile = value;
            }
        }
        internal Int32 PortTCP
        {
            set
            {
                _PortTCP = value;
            }
        }

        internal Dictionary<string, string> listFiles
        {
            set
            {
                _listFiles = value;
            }
        }

        //internal string[] Files
        //{
        //    set
        //    {
        //        _Files = value;
        //    }
        //}
    }
}
