using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using IotAirClock.LCD1602;
using Windows.ApplicationModel.Background;
using IotAirClock._18D20;

namespace IotAirClock
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var def = taskInstance.GetDeferral();

            var lcd1602 = LCD1602Screen.Instance();
            var ds18b20 = DS18D20Sensor.Instance();

            lcd1602.WriteLine("My Air Clock!", 0);
            lcd1602.WriteLine((await ds18b20.GetTemperatureAsync()).ToString() + "C", 1);

            def.Complete();
        }
    }
}
