namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.23
    public class PortInputFormatSingleMessage : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public uint DeltaInterval { get; set; }
        public bool NotificationEnabled { get; set; }
    }
}