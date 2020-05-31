namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.17
    public class PortInputFormatSetupSingleMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public uint DeltaInterval { get; set; }
        public bool NotificationEnabled { get; set; }
    }
}