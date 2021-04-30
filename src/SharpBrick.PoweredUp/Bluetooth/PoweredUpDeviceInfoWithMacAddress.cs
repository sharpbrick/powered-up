using System;
using System.Linq;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public class PoweredUpBluetoothDeviceInfoWithMacAddress : IPoweredUpBluetoothDeviceInfo, IPoweredUpBluetoothDeviceInfoWithMacAddress
    {
        public string Name { get; set; }
        public byte[] ManufacturerData { get; set; }

        public byte[] MacAddress => UInt64MacAddressToByteArray(MacAddressAsUInt64);

        public ulong MacAddressAsUInt64 { get; set; }

        public bool Equals(IPoweredUpBluetoothDeviceInfo other)
        {
            if (other != null && other is PoweredUpBluetoothDeviceInfoWithMacAddress otherMacAddress)
            {
                return this.MacAddressAsUInt64 == otherMacAddress.MacAddressAsUInt64;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
            => MacAddressAsUInt64.GetHashCode();


        /// <summary>
        /// Convert a UInt64 into a Byte Array while keeping endianess out of the game.
        /// </summary>
        /// <param name="myulong">the ulong to be converted</param>
        /// <returns></returns>
        private static byte[] UInt64MacAddressToByteArray(ulong myulong)
        {
            var result = new byte[6];
            for (var i = 5; i >= 0; i--)
            {
                result[i] = (byte)(myulong % 256);
                myulong /= 256;
            }
            return result;
        }
    }
}