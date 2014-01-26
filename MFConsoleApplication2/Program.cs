using System;
using Microsoft.SPOT;
using System.Threading;

using Microsoft.SPOT.Hardware;
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

          for (UInt16 i = 0; i < 10; i++)
          {
              pwm.setPWM(10, duty1, phase1);

              // send pwm settings until successfull transmitted
              // even if this usually works the first time
              while (!pwm.startPWM(true)) ;

              Debug.Print("phaseshift");

              // phase shift each PWM channel
              for (int j = 0; j < 4; j++)
              {
                  phase1[j] *= 2;
                  phase1[j] %= 100;
              }
                  

              Thread.Sleep(2000);
          }

          // turn all PWM channels off for 2 seconds
          while(!pwm.startPWM(false));
          Thread.Sleep(2000);

          // start them again with last configured settings
          while (!pwm.startPWM(true)) ;
        }
    }
}
