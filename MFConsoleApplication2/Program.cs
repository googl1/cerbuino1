using System;
using Microsoft.SPOT;
using System.Threading;

namespace CerbuinoSPI
{
    public class Program
    {
      public static void Main()
        {
          // init connection to STM32F4Discovery Board
          DiscoveryPWM pwm = new DiscoveryPWM();

          // PWM Setting (values from 0 to 100):
          // duty cycles for 4 channels
          UInt16[] duty1 = {50,50,50,50};
          // phase shifts for 4 channels
          UInt16[] phase1 = {1,2,3,4};

          Debug.Print("Sending test pattern");
        
          for (UInt16 i = 0; i < 5; i++)
          {
              pwm.setPWM(100, duty1, phase1);

              // send pwm settings until successfull transmitted
              // even if this usually works the first time
              while (!pwm.startPWM(true)) ;

              Debug.Print("phaseshift");
              for (int j = 0; j < 4; j++)
              {
                  phase1[j] *= 2;
                  phase1[j] %= 100;
              }
                  

              Thread.Sleep(2000);
          }
          while(!pwm.startPWM(false));
          Thread.Sleep(2000);
          while (!pwm.startPWM(true)) ;
        }
    }
}
