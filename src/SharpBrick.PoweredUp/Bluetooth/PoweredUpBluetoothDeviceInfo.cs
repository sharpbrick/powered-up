namespace SharpBrick.PoweredUp.Bluetooth
{
    public sealed class PoweredUpBluetoothDeviceInfo
    {
        public ulong BluetoothAddress { get; set; }
        public byte[] ManufacturerData { get; set; }
        public string Name { get; set; }
    }
}