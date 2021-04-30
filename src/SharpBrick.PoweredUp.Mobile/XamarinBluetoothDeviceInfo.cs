using System;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinBluetoothDeviceInfo : IPoweredUpBluetoothDeviceInfo, IPoweredUpBluetoothDeviceInfoWithMacAddress
    {
        public string Name { get; set; }
        public byte[] ManufacturerData { get; set; }

        public byte[] MacAddress => throw new NotImplementedException();

        public ulong MacAddressAsUInt64 { get; set; }

        public bool Equals(IPoweredUpBluetoothDeviceInfo other)
        {
            if (other != null && other is XamarinBluetoothDeviceInfo otherXamarin)
            {
                return this.MacAddressAsUInt64 == otherXamarin.MacAddressAsUInt64;
            }
            else
            {
                return false;
            }
        }
    }

}
