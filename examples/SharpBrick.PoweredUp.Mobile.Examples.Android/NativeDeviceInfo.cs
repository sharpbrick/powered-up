using System;
using System.Linq;

namespace SharpBrick.PoweredUp.Mobile.Examples.Droid
{
    public class NativeDeviceInfo : INativeDeviceInfo
    {
        private static NativeDeviceInfo _instance;
        
        public static NativeDeviceInfo Current
        {
            get { return _instance ??= new NativeDeviceInfo(); }
        }

        public INativeDevice GetNativeDevice(object deviceInfoObject)
        {
            if (deviceInfoObject is Android.Bluetooth.BluetoothDevice btDevice && btDevice.Address != null)
            {
                return new NativeDevice()
                {
                    MacAddress = btDevice.Address,
                    MacAddressNumeric = Convert.ToUInt64(btDevice.Address.Replace(":", ""), 16)
                };
            }

            if (deviceInfoObject is ulong deviceAddress)
            {
                string address = $"000000000000{deviceAddress:X}";
                address = address.Substring(address.Length - 12);
                return new NativeDevice()
                {
                    MacAddress = string.Join(":", Enumerable.Range(0, 6).Select(i => address.Substring(i * 2, 2))),
                    MacAddressNumeric = deviceAddress
                };

            }

            return null;
        }
    }
}