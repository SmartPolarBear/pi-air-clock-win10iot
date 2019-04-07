using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotAirClock.LCD1602
{
    public enum LCD1602Configs
    {
        Data8Line = 0x33,
        Data4Line = 0x32,
        Lines2_Dots5x7 = 0x28,
        Cursor = 0x0C
    }
}
