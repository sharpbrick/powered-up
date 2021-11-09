namespace SharpBrick.PoweredUp.Protocol.Messages;

public record PortOutputCommandFeedback(byte PortId, PortFeedback Feedback);
