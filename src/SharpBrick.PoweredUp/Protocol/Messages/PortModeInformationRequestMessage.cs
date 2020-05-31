namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.16.1
    public class PortModeInformationRequestMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public PortModeInformationType InformationType { get; set; }
    }
}