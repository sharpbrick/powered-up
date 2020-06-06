namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortOutputCommandFeedbackMessage : PoweredUpMessage
    {
        public PortOutputCommandFeedback[] Feedbacks { get; set; }
    }
}