using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using IotAirClock.LCD1602;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Sensors.Dht;
using IotAirClock.DHT11;

namespace IotAirClock
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();

            var lcd1602 = LCD1602Screen.Instance();
            lcd1602.WriteLine("My Air Clock!", 0);

            var dht11 = new DHT11Sensor(5);
            while(true)
            {
                dht11.Read(out var data);
                lcd1602.WriteLine($"{data.Temperature} - {data.Humidity}", 1);
            }

            def.Complete();
        }
    }
}
