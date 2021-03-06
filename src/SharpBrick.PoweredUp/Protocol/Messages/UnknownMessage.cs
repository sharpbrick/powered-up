using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record UnknownMessage(byte MessageTypeAsByte, byte[] Data) : LegoWirelessMessage((MessageType)MessageTypeAsByte)
    {
        public override string ToString()
            => $"Unknown Message Type: {(MessageType)this.MessageType} Length: {this.Length} Content: {BytesStringUtil.DataToString(this.Data)}";
    }
}