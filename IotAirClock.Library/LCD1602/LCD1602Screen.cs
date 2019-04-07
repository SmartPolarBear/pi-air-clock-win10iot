using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace IotAirClock.LCD1602
{
    public sealed class LCD1602Screen
    {
        private static readonly LCD1602Screen instance = new LCD1602Screen();
        public static LCD1602Screen Instance()
        {
            return instance;
        }

        private const int slaveAddress = 0x27;
        private I2cDevice lcd1602;

        private LCD1602Screen()
        {
            var tsk = ConstructAsync();
        }

        private void SendByte(byte data)
        {
            lcd1602.Write(new byte[] { NoBacklight ? Convert.ToByte(data) : Convert.ToByte(data | 0x08) });
        }

        private void Send(byte data, bool rs, bool rw, bool en)
        {
            byte flag = Convert.ToByte((Convert.ToByte(en) << 2)
                + (Convert.ToByte(rw) << 1)
                + (Convert.ToByte(rs) << 0));

            byte high = Convert.ToByte((data & 0b11110000) | flag);
            SendByte(high);
            high &= 0b11111011;
            SendByte(high);

            byte low = Convert.ToByte(((data & 0b00001111) << 4) | flag);
            SendByte(low);
            low &= 0b11111011;
            SendByte(low);
        }

        private void SendCommand(byte command)
        {
            Send(command, false, false, true);
        }

        private void SendData(byte data)
        {
            Send(data, true, false, true);
        }

        private async Task ConstructAsync()
        {
            var i2cDevSelector = I2cDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(i2cDevSelector);
            var lcd1602Settings = new I2cConnectionSettings(slaveAddress)
            {
                BusSpeed = I2cBusSpeed.StandardMode,
            };

            lcd1602 = await I2cDevice.FromIdAsync(devices.FirstOrDefault()?.Id ?? string.Empty,
                lcd1602Settings);

            SendCommand(Convert.ToByte(LCD1602Configs.Data8Line));
            SendCommand(Convert.ToByte(LCD1602Configs.Data4Line));
            SendCommand(Convert.ToByte(LCD1602Configs.Lines2_Dots5x7));
            SendCommand(Convert.ToByte(LCD1602Configs.Cursor));

            this.Clear();

            lcd1602.Write(new byte[] { 0x08 });
        }

        public void Clear()
        {
            SendCommand(Convert.ToByte(LCD1602Commands.Clear));
        }

        public void WriteLine(string text, int line)
        {
            Write(text, new Windows.Graphics.PointInt32() { X = 0, Y = line });
        }

        public void Write(string text, Windows.Graphics.PointInt32 pos)
        {
            int x = Math.Min(15, Math.Max(pos.X, 0));
            int y = Math.Min(1, Math.Max(0, pos.Y));

            byte cursorAddress = Convert.ToByte(0x80 + 0x40 * y + x);
            SendCommand(cursorAddress);
            foreach (var c in text)
            {
                SendData(Convert.ToByte(c));
            }
        }

        private bool noBacklight = false;

        public bool NoBacklight
        {
            get { return noBacklight; }
            set { noBacklight = value; }
        }

    }
}
