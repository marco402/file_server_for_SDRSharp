//https://docs.microsoft.com/fr-fr/archive/blogs/dawate/intro-to-audio-programming-part-3-synthesizing-simple-wave-audio-using-c
namespace SDRSharp.RTLTCP
{
    internal class WaveHeader
    {
        internal string sGroupID; // RIFF
        internal uint dwFileLength; // total file length minus 8, which is taken up by RIFF
        internal string sRiffType; // always WAVE
        /// <summary>
        /// Initializes a WaveHeader object with the default values.
        /// </summary>
        internal WaveHeader()
        {
            dwFileLength = 0;
            sGroupID = "RIFF";
            sRiffType = "WAVE";
        }
    }
}
