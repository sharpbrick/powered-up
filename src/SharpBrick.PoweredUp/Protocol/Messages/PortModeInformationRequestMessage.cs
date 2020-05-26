namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.16.1
    public class PortModeInformationRequestMessage : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public byte InformationType { get; set; }
    }
}