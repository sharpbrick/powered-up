using System;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public abstract record HubAttachedIOMessage(byte PortId, HubAttachedIOEvent Event) : LegoWirelessMessage(MessageType.HubAttachedIO);

    // spec chapter: 3.8.1
    public record HubAttachedIOForAttachedDeviceMessage(byte PortId, DeviceType IOTypeId, Version HardwareRevision, Version SoftwareRevision) : HubAttachedIOMessage(PortId, HubAttachedIOEvent.AttachedIO)
    {
        public override string ToString()
            => $"Attached IO - Port {HubId}/{PortId} of device type {IOTypeId} (HW: {HardwareRevision} / SW: {SoftwareRevision})";
    }

    // spec chapter: 3.8.1
    public record HubAttachedIOForAttachedVirtualDeviceMessage(byte PortId, DeviceType IOTypeId, byte PortAId, byte PortBId) : HubAttachedIOMessage(PortId, HubAttachedIOEvent.AttachedVirtualIO)
    {
        public override string ToString()
            => $"Attached Virtual IO - Port {HubId}/{PortId} using ports {PortAId} + {PortBId} of device type {IOTypeId}";
    }

    // spec chapter: 3.8.1
    public record HubAttachedIOForDetachedDeviceMessage(byte PortId) : HubAttachedIOMessage(PortId, HubAttachedIOEvent.DetachedIO)
    {
        public override string ToString()
            => $"Dettached IO - Port {HubId}{PortId}";
    }
}