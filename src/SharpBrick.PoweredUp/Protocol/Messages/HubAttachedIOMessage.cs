namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public abstract class HubAttachedIOMessage : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Event { get; set; }
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForAttachedDeviceMessage : HubAttachedIOMessage
    {
        public ushort IOTypeId { get; set; }
        public int HardwareRevision { get; set; }
        public int SoftwareRevision { get; set; }
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForAttachedVirtualDeviceMessage : HubAttachedIOMessage
    {
        public ushort IOTypeId { get; set; }
        public byte PortIdA { get; set; }
        public byte PortIdB { get; set; }
    }

    // spec chapter: 3.8.1
    public class HubAttachedIOForDetachedDeviceMessage : HubAttachedIOMessage
    { }
}