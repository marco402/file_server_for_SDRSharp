using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace SDRSharp.RTLTCP
{
    internal static unsafe class WavRecorder
    {
        internal enum RecordType : int
        {
            RAW = 0,
            WAV
        }
        internal static void WriteBufferToWav(string filePath, float[] buffer, double sampleRate)
        {
            WaveHeader header = new WaveHeader();
            WaveFormatChunk<float> format;
            WaveDataChunk<float> data;
            int nbChannel = 2;
            format = new WaveFormatChunk<float>((short)nbChannel, (uint)sampleRate);
            data = new WaveDataChunk<float>((uint)buffer.Length); //* nbChannel
            double coefficient = ClassUtils.GetMaxiTabFloat(buffer);  //not ok with .Max() not abs value
            if (coefficient > 0.0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    data.shortArray[i] = (float)(buffer[i] / coefficient);  //  I    from -1 to +1
                }
            }
            if (coefficient > 0.0)
            {
                WriteFileWav(filePath, header, format, data);
            }
            else
            {
                _ = MessageBox.Show("No record, all values = 0", "information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        internal static void WriteBufferToWav(string filePath, byte[] buffer, double sampleRate)
        {
            WaveHeader header = new WaveHeader();
            WaveFormatChunk<float> format;
            WaveDataChunk<float> data;
            int nbChannel = 2;
            format = new WaveFormatChunk<float>((short)nbChannel, (uint)sampleRate);
            data = new WaveDataChunk<float>((uint)buffer.Length);
            if (buffer.Count() == 0)
            {
                _ = MessageBox.Show("No record for:" + filePath, "information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                float[] tabFloat = new float[buffer.Length];
                bool ret = ClassUtils.ConvertCu8ToFloat(buffer, tabFloat);
                if (ret)
                {
                    data.shortArray = tabFloat; //------------------------->??? without readOnly
                    WriteFileWav(filePath, header, format, data);
                }

                else
                {
                    _ = MessageBox.Show("No record, all values = 0", "information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private static void WriteFileWav(string filePath, WaveHeader header, WaveFormatChunk<float> format,
        WaveDataChunk<float> data)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write(header.sGroupID.ToCharArray());
                    writer.Write(header.dwFileLength);
                    writer.Write(header.sRiffType.ToCharArray());
                    writer.Write(format.sChunkID.ToCharArray());
                    writer.Write(format.dwChunkSize);
                    writer.Write(format.wFormatTag);
                    writer.Write(format.wChannels);
                    writer.Write(format.dwSamplesPerSec);
                    writer.Write(format.dwAvgBytesPerSec);
                    writer.Write(format.wBlockAlign);
                    writer.Write(format.wBitsPerSample);
                    writer.Write(data.sChunkID.ToCharArray());
                    writer.Write(data.dwChunkSize);
                    foreach (float dataPoint in data.shortArray)
                    {
                        writer.Write(dataPoint);
                    }

                    _ = writer.Seek(4, SeekOrigin.Begin);
                    uint filesize = (uint)writer.BaseStream.Length;
                    writer.Write(filesize - 8);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "WavRecorder->Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        internal static void WriteByte(string fileName, byte[] dataCu8)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            try
            {
                using (BinaryWriter writer = new BinaryWriter(fileStream))
                {
                    writer.Write(dataCu8);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        internal static int GetSampleRateFromName(string fileName)
        {
            string sampleRateStr;
            fileName = Path.GetFileName(fileName);
            sampleRateStr = GetString(fileName, "k");
            if (sampleRateStr != "" && float.TryParse(sampleRateStr, out float sampleRatek))
            {
                return (int)sampleRatek * 1000;
            }
            sampleRateStr = GetString(fileName, "M");
            return sampleRateStr != "" && float.TryParse(sampleRateStr, out float sampleRateM)
                ? (sampleRateM * 1000000) <= 3200000 ? (int)(sampleRateM * 1000000) : -1
                : -1;
        }
        internal static string GetFrequencyFromName(string fileName)
        {
            string[] units = { "M", "Khz", "Hz", "k", "m", "khz", "HZ", "K" };
            string freqStr;
            fileName = Path.GetFileName(fileName);
            foreach (string unit in units)
            {
                freqStr = GetString(fileName, unit);
                if (freqStr != "")
                {
                    return freqStr + unit;
                }
            }
            return "";
        }
        internal static string GetString(string fileName, string unit)
        {
            int fin = fileName.Length - 3;
            string retString = "";
            for (int i = fin; i > 0; i--)
            {
                if (fileName.Substring(i, 1) == unit)
                {
                    int lastCar = i - 1;
                    for (--i; i > 0; i--)
                    {
                        if (fileName.Substring(i, 1) == "_")
                        {
                            int startCar = i + 1;
                            retString = fileName.Substring(startCar, lastCar - startCar + 1);
                            break;
                        }
                    }
                }
                retString = retString.Replace(".", ",");
                if (float.TryParse(retString, out _))   //found for another unit
                {
                    break;
                }
            }
            return retString;
        }
    }
}

