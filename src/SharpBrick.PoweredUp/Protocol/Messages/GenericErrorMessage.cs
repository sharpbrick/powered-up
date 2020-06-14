namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.9.1
    public class GenericErrorMessage : PoweredUpMessage
    {
        public byte CommandType { get; set; }
        public ErrorCode ErrorCode { get; set; }

        public override string ToString()
            => $"Error - {ErrorCode} from {(MessageType)CommandType}";
    }
}