using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace IotAirClock._18D20
{
    public sealed class DS18D20Sensor
    {
        private static readonly DS18D20Sensor instance = new DS18D20Sensor();
        public static DS18D20Sensor Instance()
        {
            return instance;
        }

        private SerialDevice port = null;
        private DataWriter writer = null;
        private DataReader reader = null;
        private string deviceId = "";

        private DS18D20Sensor()
        {
            var tsk = ConstructAsync();
        }

        private async Task ConstructAsync()
        {
            string selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);
            deviceId = devices.FirstOrDefault()?.Id;
        }

        private async Task ResetAsync()
        {
            if (port != null)
            {
                port.Dispose();
            }


            try
            {
                port = await SerialDevice.FromIdAsync(deviceId);
            }
            catch (Exception)
            {
                throw;
            }

            port.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            port.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            port.BaudRate = 9600;
            port.Parity = SerialParity.None;
            port.StopBits = SerialStopBitCount.One;
            port.DataBits = 8;
            port.Handshake = SerialHandshake.None;

            byte ret = 0xFF;

            try
            {
                writer = new DataWriter(port.OutputStream);
                writer.WriteByte(0XF0);
                await writer.StoreAsync();

                reader = new DataReader(port.InputStream);
                //port.InputStream.ReadAsync
                //await reader.LoadAsync(1);
                //ret = reader.ReadByte();
            }
            catch (Exception)
            {

                throw;
            }

            if (ret == 0xFF)
            {
                throw new InvalidOperationException("Nothing connected to UART");
            }
            else if (ret == 0xF0)
            {
                throw new InvalidOperationException("No 1-wire devices");
            }
            else
            {
                port.Dispose();
                port = null;

                try
                {
                    port = await SerialDevice.FromIdAsync(deviceId);
                }
                catch (Exception)
                {
                    throw;
                }


                port.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                port.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                port.BaudRate = 115200;
                port.Parity = SerialParity.None;
                port.StopBits = SerialStopBitCount.One;
                port.DataBits = 8;
                port.Handshake = SerialHandshake.None;
                writer = new DataWriter(port.OutputStream);
                reader = new DataReader(port.InputStream);
            }
        }

        private async Task WriteByteAsync(byte b)
        {
            for (byte i = 0; i < 8; i++, b = Convert.ToByte(b >> 1))
            {
                await OneWireBitAsync(Convert.ToByte(b & 0x01));
            }
        }

        private async Task<byte> OneWireBitAsync(byte b)
        {
            byte bit = Convert.ToByte(b > 0 ? 0xFF : 0x00);
            writer.WriteByte(bit);
            await writer.StoreAsync();
            await reader.LoadAsync(1);
            var data = reader.ReadByte();
            return Convert.ToByte(data & 0xFF);
        }

        private async Task<byte> ReadByteAsync()
        {
            byte val = 0;
            for(byte i=0;i<8;i++)
            {
                val = Convert.ToByte((val >> 1) + 0x80 * await OneWireBitAsync(1));
            }
            return val;
        }
        public void Shutdown()
        {
            if (port != null)
            {
                port.Dispose();
                port = null;
            }
        }

        public async Task<double> GetTemperatureAsync()
        {
            double temperature = -150;

            try
            {
                await ResetAsync();
            }
            catch (Exception)
            {

                throw;
            }

            await WriteByteAsync(0xCC);
            await WriteByteAsync(0x44);

            await Task.Delay(750);

            await ResetAsync();
            await WriteByteAsync(0xCC);
            await WriteByteAsync(0xBE);

            byte lsb = await ReadByteAsync();
            byte msb = await ReadByteAsync();

            await ResetAsync();

            temperature = ((msb << 8) + lsb) * 0.0625;
            return temperature;
        }
    }
}
