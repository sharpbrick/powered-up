using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class UnknownMessage : PoweredUpMessage
    {
        public byte[] Data { get; set; }

        public override string ToString()
            => $"Unknown Message Type: {(MessageType)this.MessageType} Length: {this.Length} Content: {BytesStringUtil.DataToString(this.Data)}";
    }
}