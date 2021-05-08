using System.Linq;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothDeviceInfoWithMacAddress
    {
        byte[] MacAddress { get; }
    }

    public static class IPoweredUpBluetoothDeviceInfoWithMacAddressExtensions
    {
        public static string ToIdentificationString(this IPoweredUpBluetoothDeviceInfoWithMacAddress self)
            => string.Join(":", self.MacAddress.Select(b => b.ToString("X2")).ToArray());
    }
}