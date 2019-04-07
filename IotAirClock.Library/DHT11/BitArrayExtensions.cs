using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotAirClock.Library.DHT11
{
    public static class BitArrayExtensions
    {
        public static long ToLong(this BitArray bits)
        {
            long ret = 0;
            for(int i=0;i<bits.Length;i++)
            {
                if (bits[i])
                {
                    ret += Convert.ToInt64(Math.Pow(2, i));
                }
            }
            return ret;
        }
    }
}
