using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace CerbuinoSPI
{
    class DiscoveryPWM
    {
        private const int SPI_OFFSET = 4;
        private SPI MySPI;
        private UInt16 f = 10;
        private UInt16[] duty = { 0, 0, 0, 0 };
        private UInt16[] phase = { 0, 0, 0, 0 };

        // PWM settings for startPWM(false)
        private const UInt16 off_f = 10;
        private UInt16[] off_duty = { 0, 0, 0, 0 };
        private UInt16[] off_phase = { 0, 0, 0, 0 };


        OutputPort NRST;
        /* Setup SPI Connection to Discovery Board      *
         * This also resets the Discovery Board if      *
         * the NRST Pin is conncted.                    *
         * Pin Setup:                                   *
         * SCK  = 13                                    *
         * MISO = 12                                    *
         * MOSI = 11                                    *
         * CS   = 10                                    *
         * NRST = 3                                     */
        public DiscoveryPWM()
        {
            //PA15 is "10"
            //PB11 is Socket 1, Pin 5
            SPI.Configuration MyConfig =
                 new SPI.Configuration(GHI.Hardware.FEZCerb.Pin.PB11,
                     false, 0, 0, true, false, 1000, SPI.SPI_module.SPI1);

            MySPI = new SPI(MyConfig);


            // restart Discovery Board in case it was running before SPI configuration
            NRST = new OutputPort(GHI.Hardware.FEZCerb.Pin.PA14, true);

            startPWM(false);
            NRST.Write(false);
            Thread.Sleep(20);
            NRST.Write(true);
            Thread.Sleep(100);
        }
        
        /* Starts and stops the PWM generation according to state.      *
         * To start the PWM generation, PWM settings should allready be *
         * configured, using setPWM().                                  */
        public bool startPWM(bool state)
        {
            if (state)
                return sendPWM(f, duty, phase);
            return sendPWM(off_f, off_duty, off_phase);
        }

        /* Saves a PWM setting.                                                 *
         * f        : the frequency of the 4 PWM channels                       *
         * duty     : array of size 4 with the duty cycles for PWM channels     *
         *            0-3, values from 0-100                                    *
         * phase    : array of size 4 with the phase shifts for PWM channels    *
         *            0-3, values from 0-100                                    */
        public void setPWM(UInt16 f, UInt16[] duty, UInt16[] phase)
        {
            if (duty.Length != 4)
                throw new ArgumentException("duty[] needs to be of length 4");
            if (phase.Length != 4)
                throw new ArgumentException("phase[] needs to be of length 4");
            this.f = f;
            this.duty = duty;
            this.phase = phase;
        }

        /* Send PWM settings to Discovery Board, not to be used from outside this class  */
        private bool sendPWM(UInt16 f, UInt16[] duty, UInt16[] phase)
        {
            if (duty == null || phase == null)
                throw new System.NullReferenceException();
            
            for (int i = 0; i < 4; i++)
            {
                if (duty[i] > 100)
                    throw new System.ArgumentOutOfRangeException("duty["
                        + i.ToString() + "] needs to be between 0 and 100");
                if (phase[i] > 100)
                    throw new System.ArgumentOutOfRangeException("phase["
                        + i.ToString() + "] needs to be between 0 and 100");
            }

            if (f < 10 || f > 1000)
                throw new System.ArgumentOutOfRangeException("f needs to be between 10 and 1000");
   
            UInt16[] rx16 = new UInt16[9 + SPI_OFFSET];
            UInt16[] tx16 = new UInt16[9 + SPI_OFFSET];

            tx16[0] = f;
            duty.CopyTo(tx16, 1);
            phase.CopyTo(tx16, 5);

            tx16[9] = 0;
            for (int i = 0; i < 9; i++) 
                tx16[9] ^= tx16[i];

            for (int i = 10; i < 9 + SPI_OFFSET; i++) {
                tx16[i] = 0x00;
            }
                        
            MySPI.WriteRead(tx16, rx16);

            for (int j = 0; j < 9 + SPI_OFFSET; j++)
            {
                if (rx16[j] == tx16[9])
                    return true;
            }

            return false;
        }
    }
}
