namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.1
    public abstract class CommonHubAttachedIOHeader : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Event { get; set; }
    }
}