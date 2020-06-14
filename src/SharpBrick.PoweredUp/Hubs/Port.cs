namespace SharpBrick.PoweredUp
{
    public class Port
    {
        public IPoweredUpDevice _device;
        public byte PortId { get; }
        public string FriendlyName { get; }
        public bool ExternalPort { get; }
        public DeviceType? ExpectedDevice { get; }
        public bool IsVirtual { get; }

        public bool IsDeviceAttached => (_device != null);
        public DeviceType? DeviceType { get; private set; }

        public Port(byte portId, string friendlyName, bool externalPort, DeviceType? expectedDevice = null, bool isVirtual = false)
        {
            PortId = portId;
            FriendlyName = friendlyName;
            ExternalPort = externalPort;
            ExpectedDevice = expectedDevice;
            IsVirtual = isVirtual;
        }

        public TDevice GetDevice<TDevice>() where TDevice : class, IPoweredUpDevice
            => _device as TDevice;

        public void AttachDevice(IPoweredUpDevice device, DeviceType type)
        {
            DeviceType = type;
            _device = device;
        }

        public void DetachDevice()
        {
            DeviceType = null;
            _device = null;
        }
    }
}