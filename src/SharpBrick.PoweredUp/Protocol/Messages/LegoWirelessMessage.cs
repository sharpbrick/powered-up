using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.1
// Length: spec chapter: 3.2; encoded as byte or ushort
public abstract record LegoWirelessMessage(MessageType MessageType)
{
    public ushort Length { get; set; }
    public byte HubId { get; set; }
}

// spec chapter: 3.9.1
public record GenericErrorMessage(byte CommandType, ErrorCode ErrorCode)
    : LegoWirelessMessage(MessageType.GenericErrorMessages)
{
    public override string ToString()
        => $"Error - {ErrorCode} from {(MessageType)CommandType}";
}

public record UnknownMessage(byte MessageTypeAsByte, byte[] Data)
    : LegoWirelessMessage((MessageType)MessageTypeAsByte)
{
    public override string ToString()
        => $"Unknown Message Type: {(MessageType)this.MessageType} Length: {this.Length} Content: {BytesStringUtil.DataToString(this.Data)}";
}
