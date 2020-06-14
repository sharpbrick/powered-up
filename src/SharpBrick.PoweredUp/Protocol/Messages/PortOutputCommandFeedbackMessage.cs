using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortOutputCommandFeedbackMessage : PoweredUpMessage
    {
        public PortOutputCommandFeedback[] Feedbacks { get; set; }

        public override string ToString()
            => $"Port Output Command Feedback - " + string.Join(",", this.Feedbacks.Select(f => $"Port {this.HubId}/{f.PortId} -> {f.Feedback}"));
    }
}