using System;
using System.Runtime.InteropServices;
namespace SDRSharp.RTLTCP
{
    internal class WaveDataChunk<T> where T : struct, IConvertible
    {
        internal readonly string sChunkID;     // "data"
        internal readonly uint dwChunkSize;    // Length of data chunk in bytes
        internal T[] shortArray;  // 8-bit audio          readonly
        /// <summary>
        /// Initializes a new data chunk with a specified capacity.
        /// </summary>
        internal WaveDataChunk(uint capacity)
        {
            shortArray = new T[capacity];
            dwChunkSize = (uint)(Marshal.SizeOf<T>() * capacity);
            //problem to float /complex
            sChunkID = "data";
        }
    }
}
