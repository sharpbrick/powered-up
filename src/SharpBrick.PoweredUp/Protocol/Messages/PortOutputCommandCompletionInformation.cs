namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public enum PortOutputCommandCompletionInformation : byte
    {
        NoAction = 0x00,
        CommandFeedback = 0x01,
    }
}