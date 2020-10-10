using System;
using System.Linq;

namespace SharpBrick.PoweredUp.BlueZ.Utilities
{
    public class BluetoothAddressFormatter
    {
        public ulong ConvertToInteger(string bluetoothMacAddress) 
            => Convert.ToUInt64(bluetoothMacAddress.Replace(":", ""), 16);

        public string ConvertToMacString(ulong bluetoothAddress)
            => string.Join(":", BitConverter.GetBytes(bluetoothAddress).Reverse().Select(b => b.ToString("X2"))).Substring(6);
    }
}
