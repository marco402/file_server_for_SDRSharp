
##           SDRSharp.RTLTCP.dll  

Version 2.0.0.0 January 2026.  

Multiple file replay for SDRSharp source= RTL-SDR(TCP).Click on 'configure source'.  

To use this software, a version of SDRSharp must be used where SDRSharp.RTLTCP.dll is located and replace this file.  

For future versions of SDRSharp, it would be interesting to have a "user source".  

"I used version 1732 available for download at this address:  
 https://archive.org/download/SDRSharp_Collection  
 
 The data is sent internally to SDRSharp without network connection.  
 
 The selected files are either .wav or any other 8-bit IQ file (e.g., .cu8 from RTL_433).  
 
 If the sample rate is not present in the name, the forced sample rate will be used.  
 if it is empty, the file will not be used.  
 The same applies to the frequency.  
 
 For start radio, it is necessary to select 1 sample rate, the frequency will be set for each file.  
 
 As for the network version:  
     Number of emissions for each file.  
     Number of emissions for all files.  
     Tempo between 2 emissions.  

Frequency  
  0 to 2.5Ghz(MaximumTunableFrequency)  
  For SDRSharp:  
    if file is .wav, frequency always Hz or hz as SDRSharp->'Baseband : Simple recorder'.  
  For RTL_433:  
    433.92M : A decimal number suffixed with M denotes the center frequency.  

Sample rate  
  1000k : A decimal number suffixed with k denotes the sample rate.  
  or M for mega.  
  excep .wav sample rate have not maximum.  
