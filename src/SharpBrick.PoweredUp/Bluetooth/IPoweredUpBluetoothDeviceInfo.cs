using System;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothDeviceInfo : IEquatable<IPoweredUpBluetoothDeviceInfo>
    {
        string Name { get; }
        byte[] ManufacturerData { get; }
    }
}