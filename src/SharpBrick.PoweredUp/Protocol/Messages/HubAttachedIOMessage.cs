using System;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public abstract class HubAttachedIOMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public HubAttachedIOEvent Event { get; set; }
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForAttachedDeviceMessage : HubAttachedIOMessage
    {
        public DeviceType IOTypeId { get; set; }
        public Version HardwareRevision { get; set; }
        public Version SoftwareRevision { get; set; }

        public override string ToString()
            => $"Attached IO - Port {HubId}/{PortId} of device type {IOTypeId} (HW: {HardwareRevision} / SW: {SoftwareRevision})";
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForAttachedVirtualDeviceMessage : HubAttachedIOMessage
    {
        public DeviceType IOTypeId { get; set; }
        public byte PortAId { get; set; }
        public byte PortBId { get; set; }

        public override string ToString()
            => $"Attached Virtual IO - Port {HubId}/{PortId} using ports {PortAId} + {PortBId} of device type {IOTypeId}";
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForDetachedDeviceMessage : HubAttachedIOMessage
    {

        public override string ToString()
            => $"Dettached IO - Port {HubId}{PortId}";
    }
}