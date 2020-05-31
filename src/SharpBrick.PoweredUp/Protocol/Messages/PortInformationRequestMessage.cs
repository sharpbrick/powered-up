namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.15.1
    public class PortInformationRequestMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public PortInformationType InformationType { get; set; }
    }
}