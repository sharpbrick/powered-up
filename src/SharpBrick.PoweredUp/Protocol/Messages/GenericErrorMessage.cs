namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.9.1
    public record GenericErrorMessage(byte CommandType, ErrorCode ErrorCode) : LegoWirelessMessage(MessageType.GenericErrorMessages)
    {
        public override string ToString()
            => $"Error - {ErrorCode} from {(MessageType)CommandType}";
    }
}