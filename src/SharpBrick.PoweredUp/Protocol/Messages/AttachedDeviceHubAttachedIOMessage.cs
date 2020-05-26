namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public class AttachedDeviceHubAttachedIOMessage : CommonHubAttachedIOHeader
    {
        public ushort IOTypeId { get; set; }
        public int HardwareRevision { get; set; }
        public int SoftwareRevision { get; set; }
    }
}