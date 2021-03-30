using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class GenericErrorMessageEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => throw new NotImplementedException();

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
            => new GenericErrorMessage(data[0], (ErrorCode)data[1]);

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}