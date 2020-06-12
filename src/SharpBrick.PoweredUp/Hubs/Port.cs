using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public class Port
    {
        public IPowerdUpDevice _device;
        public byte PortId { get; }
        public string FriendlyName { get; }
        public bool ExternalPort { get; }
        public HubAttachedIOType? ExpectedDevice { get; }

        public bool IsDeviceAttached => (_device != null);
        public HubAttachedIOType? DeviceType { get; private set; }

        public Port(byte portId, string friendlyName, bool externalPort, HubAttachedIOType? expectedDevice = null, bool isVirtual = false)
        {
            PortId = portId;
            FriendlyName = friendlyName;
            ExternalPort = externalPort;
            ExpectedDevice = expectedDevice;
        }

        public TDevice GetDevice<TDevice>() where TDevice : class, IPowerdUpDevice
            => _device as TDevice;

        public void AttachDevice(IPowerdUpDevice device, HubAttachedIOType type)
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