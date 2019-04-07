using IotAirClock.Library.DHT11;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotAirClock.DHT11
{
    public sealed class DHT11Data
    {
        private const int BIT_SIZE = 40;
        private BitArray bits = null;

        private long checksum = 0;
        private long bareValue = 0;
        private double temperature = 0;
        private double humidity = 0;

        public DHT11Data()
        {
            bits = new BitArray(BIT_SIZE);

        }


        private void RefreshValues()
        {
            bareValue = bits.ToLong();
            checksum = ((bareValue >> 32) & 0xff) + ((bareValue >> 24) & 0xff)
                    + ((bareValue >> 16) & 0xff) + ((bareValue >> 8) & 0xff);

            humidity = ((bareValue >> 32) & 0xff) + ((bareValue >> 24) & 0xff) / 10.0;
            temperature = ((bareValue >> 16) & 0xff) + ((bareValue >> 8) & 0xff) / 10.0;
        }


        internal BitArray Bits
        {
            get => bits;
            set
            {
                bits = value;
                RefreshValues();

            }
        }

        public bool IsValid
        {
            get
            {
                RefreshValues();
                return (checksum & 0xff) == (bareValue & 0xff);
            }
        }

        public double Humidity
        {
            get
            {
                RefreshValues();
                return humidity;
            }
        }

        public double Temperature
        {
            get
            {
                RefreshValues();
                return temperature;
            }
        }
    }

    public sealed class DHT11Sensor
    {
        private const int SAMPLE_HOLD_LOW_MILLIS = 18;
        private GpioPinDriveMode inputDirveMode;
        private GpioPin pin;
        private DHT11Data lastData;

        public DHT11Sensor(int pinNumber)
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                throw new FieldAccessException();
            }

            try
            {
                pin = gpio.OpenPin(pinNumber, GpioSharingMode.Exclusive);
            }
            catch (Exception)
            {

                throw;
            }

            inputDirveMode = GpioPinDriveMode.Input;

            pin.SetDriveMode(inputDirveMode);
        }

        private int Read(DHT11Data data)
        {
            long threshold = 110L * System.Diagnostics.Stopwatch.Frequency / 1000000L;
            pin.Write(GpioPinValue.Low);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            Task.Delay(18).Wait();

            pin.SetDriveMode(inputDirveMode);

            var prevValue = pin.Read();

            const long initialRisingEdgeTimeoutMillis = 50000L;
            long endTicCount = DateTime.Now.Ticks + initialRisingEdgeTimeoutMillis;

            for (; ; )
            {
                if (DateTime.Now.Ticks > endTicCount)
                {
                    return 1;
                }

                var value = pin.Read();
                if (value != prevValue)
                {
                    if (value == GpioPinValue.High)
                    {
                        break;
                    }

                    prevValue = value;
                }
            }

            long prevTime = 0;
            const long sampleTimeoutMillis = 105000L;
            endTicCount = DateTime.Now.Ticks + sampleTimeoutMillis;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < data.Bits.Length + 1;)
            {
                if (DateTime.Now.Ticks > endTicCount)
                    return 2;

                var value = pin.Read();
                if (prevValue == GpioPinValue.High && value == GpioPinValue.Low)
                {
                    var now = stopwatch.ElapsedTicks;
                    if (i != 0)
                    {
                        var diff = now - prevTime;
                        data.Bits[data.Bits.Length - i] = diff > threshold;
                    }
                    prevTime = now;
                    i++;
                }
                prevValue = value;
            }
            if (!data.IsValid)
            {
                return 3;
            }
            return 0;
        }

        public int Read(out DHT11Data data)
        {
            data = new DHT11Data();
            int retry = 0;
            int ret = 0;
            do
            {
                ret = Read(data);
            } while (ret != 0 && (++retry) < 25);

            if (ret != 0 && lastData != null) data = lastData;
            else
            {
                lastData = data;
                Debug.WriteLine("Succeed read!");
            }

            return ret;
        }
    }
}
