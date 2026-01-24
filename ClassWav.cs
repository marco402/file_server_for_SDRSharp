/* Written by Marc Prieur (marco40_github@sfr.fr)
                                   ClassWav.cs 
                               project ServerForSDRSharp
 **************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license: 
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.
  **********************************************************************************/
using System;
using System.IO;
using System.Text;
namespace SDRSharp.RTLTCP
{
    internal class ClassWav
    {
        internal static bool GetSampleRate(string filename, ref int SampleRate, ref ulong SampleRate64, ref string message)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);
                    byte[] tabByte = BitConverter.GetBytes(reader.ReadInt32());
                    string chunkID = new ASCIIEncoding().GetString(tabByte);
                    _ = reader.ReadInt32();
                    _ = reader.ReadInt32();
                    if (chunkID == "RIFF")
                    {
                        _ = reader.ReadInt32();
                        _ = reader.ReadInt32();
                        _ = reader.ReadInt32();
                        SampleRate = reader.ReadInt32();
                        return true;
                    }
                    else if (chunkID == "RF64")
                    {
                        long riff64 = reader.ReadInt64();
                        long rf64DataSize = reader.ReadInt64();
                        _ = reader.ReadInt64();
                        _ = reader.ReadInt64();
                        _ = reader.ReadInt64();
                        _ = reader.ReadInt64();
                        SampleRate = reader.ReadInt32();
#if !RF64
                        message += $"ToDo->Type RF64 not processed:\t\n";
                        return false;
#else
                        return true;
#endif
                    }
                    message += $"wav:chunkID not found:\t\n";
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        internal static bool ReadWav(string filename, ref byte[] byteArray, ref float[] L, ref int size, bool retLAndRElseAllInL, ref string message, ref int SampleRate)
        {
            try
            {
                using (FileStream fs = File.Open(filename, FileMode.Open))
                {
                    BinaryReader reader = new BinaryReader(fs);
                    //message = filename + "\n";
                    byte[] tabByte = BitConverter.GetBytes(reader.ReadInt32());
                    string chunkID = new ASCIIEncoding().GetString(tabByte);
                    //message += $"chunkID:\t\t\t{chunkID}\n";
                    int fileSize = reader.ReadInt32();
                    //message += $"fileSize:\t\t\t{fileSize}\n";
                    tabByte = BitConverter.GetBytes(reader.ReadInt32());
                    string riffType = new ASCIIEncoding().GetString(tabByte);
                    //message += $"riffType:\t\t\t{riffType}\n";
                    int bitDepth = 0;
                    int nValues = 0;
                    int channels = 0;
                    int bytes = 0;
                    long bytes64 = 0;
                    if (chunkID == "RIFF")
                    {
                        // chunk 1
                        tabByte = BitConverter.GetBytes(reader.ReadInt32());
                        string fmtID = new ASCIIEncoding().GetString(tabByte);
                        //message += $"fmtID:\t\t\t{fmtID}\n";
                        int fmtSize = reader.ReadInt32(); // bytes for this chunk (expect 16 or 18)
                        //message += $"fmtSize:\t\t\t{fmtSize}\n";
                        // 16 bytes coming...
                        int fmtCode = reader.ReadInt16();
                        //message += $"fmtCode:\t\t\t{fmtCode}\n";
                        channels = reader.ReadInt16();
                        //message += $"channels:\t\t{channels}\n";
                        SampleRate = reader.ReadInt32();
                        //message += $"sampleRate:\t\t{SampleRate}\n";
                        int byteRate = reader.ReadInt32();
                        //message += $"byteRate:\t\t{byteRate}\n";
                        int fmtBlockAlign = reader.ReadInt16();
                        //message += $"fmtBlockAlign:\t\t{fmtBlockAlign}\n";
                        bitDepth = reader.ReadInt16();
                        //message += $"bitDepth:\t\t{bitDepth}\n";
                        if (fmtSize == 18)
                        {
                            // Read any extra values
                            int fmtExtraSize = reader.ReadInt16();
                            _ = reader.ReadBytes(fmtExtraSize);
                            //message += $"fmtExtraSize:\t\t{fmtExtraSize}\n";
                        }

                        // chunk 2
                        tabByte = BitConverter.GetBytes(reader.ReadInt32());
                        string dataID = new ASCIIEncoding().GetString(tabByte);
                        //message += $"dataID:\t\t\t{dataID}\n";
                        bytes = reader.ReadInt32();
                        //message += $"bytes:\t\t\t{bytes}\n";
                        byteArray = reader.ReadBytes(bytes);
                        int bytesForSamp = bitDepth / 8;
                        nValues = bytes / bytesForSamp;
                    }
                    else if (chunkID == "RF64")
                    {
                        bitDepth = 64;
                        tabByte = BitConverter.GetBytes(reader.ReadInt32());
                        string chunkId = new ASCIIEncoding().GetString(tabByte);
                        //message += $"fmtID:\t\t\t{chunkId}\n";
                        uint chunkSize = reader.ReadUInt32();
                        //message += $"chunkSize:\t\t{chunkSize}\n";
                        uint riffSizeLow = reader.ReadUInt32();
                        //message += $"riffSizeLow:\t\t\t{riffSizeLow}\n";
                        uint riffSizeHigh = reader.ReadUInt32();
                        //message += $"riffSizeHigh:\t\t\t{riffSizeHigh}\n";
                        ulong totalRiffSize = riffSizeLow + (riffSizeHigh * int.MaxValue);
                        //message += $"riffSize:\t\t\t{totalRiffSize}\n";
                        uint dataSizeLow = reader.ReadUInt32();
                        //message += $"dataSizeLow:\t\t\t{dataSizeLow}\n";
                        uint dataSizeHigh = reader.ReadUInt32();
                        //message += $"dataSizeHigh:\t\t\t{dataSizeHigh}\n";
                        bytes64 = dataSizeLow + (dataSizeHigh * int.MaxValue);
                        //message += $"dataSize:\t\t\t{bytes64}\n";
                        uint sampleCountLow = reader.ReadUInt32(); // bytes for this chunk (expect 16 or 18)
                        //message += $"sampleCountLow:\t\t\t{sampleCountLow}\n";
                        uint sampleCountHigh = reader.ReadUInt32(); // bytes for this chunk (expect 16 or 18)
                        //message += $"sampleCountHigh:\t\t\t{sampleCountHigh}\n";
                        ulong SampleRate64 = sampleCountLow + (sampleCountHigh * int.MaxValue);
                        //message += $"SampleRate(sampleCount):\t{SampleRate64}\n";
                        uint tableLength = reader.ReadUInt32(); // bytes for this chunk (expect 16 or 18)
                                                                //message += $"tableLength:\t\t{tableLength}\n";
                                                                //message += $"Add bitDepth:\t\t{bitDepth}\n\n";
                                                                //if (!retLAndRElseAllInL)
                                                                //    message = "";
#if !RF64
                        message += $"ToDo->Type RF64 not processed:\t\n";
                        return false;
#else
                        return true;
#endif
                    }

                    if (nValues == 0)
                    {
                        return false;
                    }

                    float[] asFloat = null;
                    switch (bitDepth)
                    {
                        case 64:
                            double[] asDouble = new double[nValues];
                            Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytes);
                            asFloat = Array.ConvertAll(asDouble, e => (float)e);
                            break;
                        case 32:
                            asFloat = new float[nValues];
                            Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytes);
                            break;
                        case 16:
                            short[] asInt16 = new short[nValues];
                            Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytes);
                            asFloat = Array.ConvertAll(asInt16, e => e / (float)(short.MaxValue + 1));
                            break;
                        //add 8 bits for 'Baseband : Simple recorder' ->8BitPcmIQ
                        case 8:
                            byte[] asByte = new byte[nValues];
                            Buffer.BlockCopy(byteArray, 0, asByte, 0, bytes);
                            asFloat = Array.ConvertAll(asByte, e => e / (float)(byte.MaxValue + 1));
                            break;
                        default:
                            message = $"error:bitDepth different of 16,32 or 64= {bitDepth}";
                            return false;
                    }
                    switch (channels)
                    {
                        case 1:
                            L = asFloat;
                            return true;
                        case 2:
                            int nSamps = nValues / 2;
                            if (!retLAndRElseAllInL)
                            {
                                Array.Resize(ref L, nValues);
                                L = asFloat;
                            }
                            size = nValues;
                            return true;
                        default:
                            message = $"error:Nb channel different of 1 or 2 ";
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"error: {ex.Message}";
                return false;
            }
        }
#if need
       private const String NAMEOUTFILEWAV = "concatenetsWav_";
        internal static String InfoWav()
        {
            using (OpenFileDialog openWav = new OpenFileDialog())
            {
                openWav.DefaultExt = "wav";
                openWav.Filter = "wav files|*.wav";
                openWav.Multiselect = true;
                if (openWav.ShowDialog() == DialogResult.OK)
                {
                    foreach (String file in openWav.FileNames)
                    {
                        byte[] byteArray = null;
                        float[] L = null;
                        float[] R = null;
                        Int32 size = 0;
                        String message = "";
                        Int32 sampleRate = 0;
                        readWav(file, ref byteArray, ref R, ref L, ref size, true, ref message, ref sampleRate);
                        MessageBox.Show(message);
                    }
                }
            }
            return "";
        }
        internal static void genereWavTest()
        {
            float[] dataIQ = new float[262144];
            for (Int32 i = 0; i < 262144; i += 2)
            {
                dataIQ[i] = 0;
                dataIQ[i + 1] = 0;
            }
            WavRecorder.WriteBufferToWav("xxxx.wav", dataIQ, 250000);
        }
        internal static String ConcateneWav(Int32 tempoBetweenFile)
        {
            String message = "";
            using (OpenFileDialog openWav = new OpenFileDialog())
            {
                openWav.DefaultExt = "wav";
                openWav.Filter = "wav files|*.wav";
                openWav.Multiselect = true;
                String fileOut = "";
                float[] data = null;
                Int32 memoSampleRate = 0;
                Int32 nbFileOK = 0;
                if (openWav.ShowDialog() == DialogResult.OK)
                {
                    Int32 size = 0;
                    byte[] byteArray = null;
                    float[] R = null;
                    float[] L = null;
                    data = new float[0]; ;
                    Int32 memDataSize = 0;
                    String files = "";
                    foreach (String file in openWav.FileNames)
                    {
                        if (file.Contains(NAMEOUTFILEWAV))
                            continue;
                        try
                        {
                            files = file;
                            Int32 sampleRate = 0;
                            Boolean ret = ClassWav.readWav(file, ref byteArray, ref R, ref L, ref size, false, ref message, ref sampleRate);

                            if (!ret)
                                message+=($"pb file { file } concatenets wav");
                            else
                            {
                                if (memoSampleRate == 0)
                                {
                                    memoSampleRate = sampleRate;
                                    fileOut = Path.GetDirectoryName(file) + "\\" + NAMEOUTFILEWAV + "_" + (memoSampleRate / 1000).ToString() + "k_" + (DateTime.Now.Date.ToString("d").Replace("/", "_") + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second) + Path.GetExtension(file);
                                }
                                if (memoSampleRate != sampleRate)
                                {
                                    message += ("Sample rate différent from the first.");
                                    message += ($"file {file}");
                                }
                                else
                                {
                                    Int32 tempo = 0;
                                    tempo = tempoBetweenFile * (memoSampleRate / 250);
                                    Int32 D = memDataSize;
                                    memDataSize += (L.Count()) + tempo;
                                    Array.Resize(ref data, memDataSize);
                                    for (Int32 i = 0; i < (L.Count()); i++)
                                        data[i + D] = (L[i]);
                                    nbFileOK++;
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            message += ($"Error {files}: {ex.Message}");
                        }
                    }
                    double coefficient = ClassUtils.GetMaxiTabFloat(data);  //not ok with .Max() not abs value
                    if (coefficient > 0.0 && coefficient != 1)
                    {
                        for (Int32 i = 0; i < (data.Count()); i++)
                            data[i] = (float)(data[i] / coefficient);  //  I    from -1 to +1
                    }
                    WavRecorder.WriteBufferToWav(fileOut, data, memoSampleRate);
                    message += ($"Completed Concatenets wav's ( {nbFileOK} / {openWav.FileNames.Count()}) to:");
                    message += ($"{fileOut}");
                }
            }
            return message;
        }
#endif
    }
}
