namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortValueSingleMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public byte[] InputValue { get; set; }
    }
}