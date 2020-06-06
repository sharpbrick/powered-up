namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortOutputCommandFeedback
    {
        public byte PortId { get; set; }

        public PortFeedback Feedback { get; set; }
    }
}