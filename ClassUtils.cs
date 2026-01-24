/* Written by Marc Prieur (marco40_github@sfr.fr)
                                ClassUtils.cs 
**************************************************************************************
 Creative Commons Attrib Share-Alike License
 You are free to use/extend this library but please abide with the CC-BY-SA license:
 Attribution-NonCommercial-ShareAlike 4.0 International License
 http://creativecommons.org/licenses/by-nc-sa/4.0/

 All text above must be included in any redistribution.
  **********************************************************************************/
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.RTLTCP
{
    internal class ClassUtils
    {
        internal static double GetMaxiTabFloat(float[] bufferPtr)
        {
            double maxi = float.MinValue;
            for (int i = 0; i < bufferPtr.Length; i++)
            {
                if (Math.Abs(bufferPtr[i]) > maxi)
                {
                    maxi = Math.Abs(bufferPtr[i]);
                }
            }

            return maxi;
        }
        internal static bool ConvertCu8ToFloat(byte[] bufferByte, float[] bufferFloat)
        {
            double maxi = bufferByte.Max() / 2;  //Max() OK all > 0
            if (maxi > 0.0)
            {
                for (int i = 0; i < bufferByte.Length; i++)
                {
                    bufferFloat[i] = (float)((bufferByte[i] - 127) / maxi);  //from -1 to +1   0--->-1  255--->1   127--->0
                }

                return true;
            }
            return false;
        }
        internal const float FLOATTOBYTE = 255f / 2f;
        internal static bool ConvertFloatToByte(float[] dataFloat, byte[] dataByte)
        {
            double coefficient = ClassUtils.GetMaxiTabFloat(dataFloat);
            if (coefficient <= 0.0)
            {
                return false;
            }

            Thread.BeginCriticalRegion();
            for (int i = 0; i < dataFloat.Length; i++)
            {
                try
                {
                    dataByte[i] = System.Convert.ToByte(FLOATTOBYTE + (dataFloat[i] / coefficient * FLOATTOBYTE));
                }
                catch
                {
                    dataByte[i] = (FLOATTOBYTE + (dataFloat[i] / coefficient * FLOATTOBYTE)) < 0 ? (byte)0 : (byte)255;
                }
            }

            Thread.EndCriticalRegion();
            return true;
        }
        internal static byte[] GetDataFile(string fileName, ref string messageRaw)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                    {
                        byte[] dataIQ = new byte[reader.BaseStream.Length];
                        dataIQ = reader.ReadBytes((int)reader.BaseStream.Length);
                        return dataIQ;
                    }
                }
                catch (Exception ex)
                {
                    messageRaw = ex.Message;
                }
            }
            return null;
        }
        //internal static Color BackColor { get; set; }
        //internal static Color ForeColor { get; set; }
        //internal static Font Font { get; set; }
        //internal static Cursor Cursor { get; set; }

    }
}
