using System;
using System.Runtime.InteropServices;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinBluetoothDeviceInfo : IPoweredUpBluetoothDeviceInfo, IPoweredUpBluetoothDeviceInfoWithMacAddress
    {
        private string _deviceIdentifier;

        public string DeviceIdentifier
        {
            get => _deviceIdentifier;
            set
            {
                _deviceIdentifier = value;
                UpdateMacAddress();
            }
        }

        public string Name { get; set; }
        
        public byte[] ManufacturerData { get; set; }

        public byte[] MacAddress { get; private set; } = new byte[0];

        public ulong MacAddressAsUInt64 { get; private set; } = 0;

        public bool Equals(IPoweredUpBluetoothDeviceInfo other)
        {
            if (other != null && other is XamarinBluetoothDeviceInfo otherXamarin)
            {
                return this.DeviceIdentifier == otherXamarin.DeviceIdentifier;
            }
            else
            {
                return false;
            }
        }

        private void UpdateMacAddress()
        {
            if (string.IsNullOrWhiteSpace(_deviceIdentifier)) return;

            if (Guid.TryParse(_deviceIdentifier, out Guid result))
            {
                // we're on an iOS device -> no mac address is revealed
                return;
            }

            if (_deviceIdentifier.Contains(":"))
            {
                MacAddress = BytesStringUtil.HexStringToByteArray(_deviceIdentifier);
                MacAddressAsUInt64 = BytesStringUtil.HexStringToUInt64(_deviceIdentifier);
                return;
            }

            if (ulong.TryParse(_deviceIdentifier, out ulong macAdressAsUlong))
            {
                MacAddressAsUInt64 = macAdressAsUlong;
                MacAddress = BytesStringUtil.UInt64MacAddressToByteArray(macAdressAsUlong);
                _deviceIdentifier = BytesStringUtil.ByteArrayToHexString(MacAddress);
            }
        }
        
    }

}
