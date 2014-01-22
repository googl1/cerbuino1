using System;
using Microsoft.SPOT;
using System.Threading;

namespace MFConsoleApplication1
{
    


    public class Program
    {

      public static void Main()
        {
          DiscoveryAnalogSignal signal = new DiscoveryAnalogSignal();
          UInt16[] half_sine = {1,0,0,0,0,0,0,0,0,0,2048,3546,4095,3546,2048};
          UInt16[] saw = {4095, 4094, 4093, 4, 5, 6, 7, 8, 9, 10, 11,12,13,14,15,16,17,
                             18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,
                             34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,
                             50,51,52,53,54,55,56,57,58,59,60,512,1024,1536,
                             2048, 2560, 3072,3584, 4095};
          String f;



          Debug.Print("Creating saw-tooth signal with f in Hz;");
          signal.sendSignal(saw);
          for (UInt16 i = 9999; i < 10000; i++)
          {
              Debug.Print("soll= " + i + " Hz");
              Debug.Print(signal.setFreq(i).ToString() + " Hz");
              Thread.Sleep(2000);
          }

          signal.sendSignal(half_sine);
          Debug.Print("Creating half sine with frequency in Hz:");
          for (UInt16 i = 1000; i < 2000; i++)
          {
              f = "soll = " + i.ToString() + " kHz";
              Debug.Print(f);
             
              Debug.Print( signal.setFreq(i).ToString());
              Thread.Sleep(500);
          }
          
        }
    }
}
