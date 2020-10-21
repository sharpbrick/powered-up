using System;
using System.Linq;

namespace SharpBrick.PoweredUp.BlueZ.Utilities
{
    public static class BluetoothAddressFormatter
    {
        public static ulong ConvertToInteger(string bluetoothMacAddress) 
            => Convert.ToUInt64(bluetoothMacAddress.Replace(":", ""), 16);

        public static string ConvertToMacString(ulong bluetoothAddress)
            => string.Join(":", BitConverter.GetBytes(bluetoothAddress).Reverse().Select(b => b.ToString("X2"))).Substring(6);
    }
}
