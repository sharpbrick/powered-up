using System;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public class PoweredUpBluetoothDeviceInfoWithMacAddress : IPoweredUpBluetoothDeviceInfo, IPoweredUpBluetoothDeviceInfoWithMacAddress
    {
        public string Name { get; set; }
        public byte[] ManufacturerData { get; set; }

        public byte[] MacAddress => new byte[1] { 99 };

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
    }
}