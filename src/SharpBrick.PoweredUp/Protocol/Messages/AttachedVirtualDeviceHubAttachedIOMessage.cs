namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public class AttachedVirtualDeviceHubAttachedIOMessage : CommonHubAttachedIOHeader
    {
        public ushort IOTypeId { get; set; }
        public byte PortIdA { get; set; }
        public byte PortIdB { get; set; }
    }
}