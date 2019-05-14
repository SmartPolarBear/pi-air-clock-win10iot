using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using IotAirClock.LCD1602;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using IotAirClock.DHT11;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using IotAirClock._18D20;
using System.Threading.Tasks;

namespace IotAirClock
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            const string empty = "                ";

            var def = taskInstance.GetDeferral();

            var lcd1602 = LCD1602Screen.Instance();
            lcd1602.WriteLine("My Air Clock!", 0);

            Task.Delay(1000).Wait();

            var dht11 = new DHT11Sensor(5);

            for (int i = 1; i != 0; i++)
            {
                lcd1602.WriteLine(empty, 0);

                dht11.Read(out var data);

                var line0 = i % 2 == 0 ? DateTime.Now.ToShortTimeString() : DateTime.Now.ToShortDateString();
                var line1 = $"{data.Temperature}C. {data.Humidity}%RH.";

                lcd1602.WriteLine(line0, 0);

                if (data.IsValid && data.Temperature != 0 && data.Humidity != 0)
                {
                    lcd1602.WriteLine(empty, 1);
                    lcd1602.WriteLine(line1, 1);
                }

                await Task.Delay(2000);
            }

            def.Complete();
        }
    }
}
