using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


namespace MFConsoleApplication1
{
    class DiscoveryAnalogSignal
    {
        const int SPI_OFFSET = 5;
        private SPI MySPI;
        UInt16[] signal = null;
        double freq;

        public DiscoveryAnalogSignal()
        {
            //PA15 is "10"
            //PA14 is Pin #3 of Gadgeteer Socket #1
            SPI.Configuration MyConfig =
                 new SPI.Configuration(GHI.Hardware.FEZCerb.Pin.PA15,
                     false, 0, 0, true, true, 1000, SPI.SPI_module.SPI1);

            MySPI = new SPI(MyConfig);
        }

        public double setFreq(double freq)
        {
            this.freq = freq;

            if (signal == null)
            {
                return freq;
            }

            if (freq * signal.Length > 10000000)
                throw new System.ArgumentOutOfRangeException("freq");
            return sendSignal(this.signal);
        }

        private UInt16 findPrescaler(double freq, int len)
        {
            double po = (double)(84000000 / (len * freq));

            double diff;
            double best_diff = po;
            UInt16 best_o = (UInt16)(po / 2);
            UInt16 o;

            for (UInt16 p = 2; p < po; p++)
            {
                o = (UInt16)(po / p);
                diff = System.Math.Abs((double)(po - (p*o)));
                if (diff < best_diff)
                {
                    best_diff = diff;
                    best_o = o;
                    if (best_diff == 0)
                        return best_o;
                }
            }
            return best_o;
        }

        /* void sendSignal(UInt16[] signal) takes an array of UInt16 values, 
         * where every value needs to be between 0 and 4095 and represents 
         * a Voltage between 0V and 2,4V at PA4 of the connected Discovery
         * Board.
         * The last two values are used to set the frequency of the signal,
         * where the first is the prescaler and the second is the overflow
         * value.
         * i.e. to create a 2kHz Signal, formed by 32 values, you could
         * use 100 as a prescaler and 10 as an overflow value:
         * 84MHz / 32 / 100 / 10 = 2kHz */
        public double sendSignal(UInt16[] signal)
        {
            if (signal == null)
                throw new System.NullReferenceException();

            int len = signal.Length;

            if (len < 1)
                throw new System.ArgumentException("signal[] needs at least 1 values");

            for (int i = 0; i < len; i++)
            {
                if (signal[i] > 4095)
                    throw new System.ArgumentOutOfRangeException("signal["
                        + i.ToString() + "]", "needs to be between 0 and 4095");
            }

            this.signal = signal;
            if (this.freq == 0)
                return 0;

            UInt16[] tx16 = new UInt16[len + SPI_OFFSET];
            UInt16[] rx16 = new UInt16[len + SPI_OFFSET];

            signal.CopyTo(tx16, 0);

            
            tx16[len] = findPrescaler(freq, len);
            tx16[len + 1] = (UInt16)(84000000 / len / freq / tx16[len]);
            tx16[len + 2] = 0x00;

            //cut signal values in bytes for SPI transport
            byte[] tx_data = new byte[(len+SPI_OFFSET) * 2];
            byte[] rx_data = new byte[(len+SPI_OFFSET) * 2];
            for (int i = 0; i < len+SPI_OFFSET; i++)
            {
                byte t = (byte)(tx16[i]);
                tx_data[2 * i] = t;
                tx_data[2 * i + 1] = (byte)(tx16[i] >> 8);
            }

            MySPI.WriteRead(tx_data, rx_data);

            for (int j = 0; j < len+SPI_OFFSET-1; j++)
                rx16[j] = (UInt16)(rx_data[2 * j+1] + (UInt16)(rx_data[2 * j + 2] << 8));

            for (int j = 0; j < len + 2; j++)
            {
                if (rx16[j + 1] != tx16[j])
                    return 0;
            }

            return (double)(84000000) / (len * tx16[len] * tx16[len + 1]);
        }
    }
}
