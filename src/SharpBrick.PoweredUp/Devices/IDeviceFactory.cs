using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Devices;

public interface IDeviceFactory
{
    IPoweredUpDevice Create(DeviceType deviceType);
    IPoweredUpDevice CreateConnected(DeviceType deviceType, ILegoWirelessProtocol protocol, byte hubId, byte portId);
}
