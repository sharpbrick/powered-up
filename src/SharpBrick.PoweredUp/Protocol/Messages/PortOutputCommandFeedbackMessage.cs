using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortOutputCommandFeedbackMessage(PortOutputCommandFeedback[] Feedbacks) : LegoWirelessMessage(MessageType.PortOutputCommandFeedback)
    {
        public override string ToString()
            => $"Port Output Command Feedback - " + string.Join(",", this.Feedbacks.Select(f => $"Port {this.HubId}/{f.PortId} -> {f.Feedback}"));
    }
}