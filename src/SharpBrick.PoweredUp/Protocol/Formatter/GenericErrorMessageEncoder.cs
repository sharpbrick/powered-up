using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class GenericErrorMessageEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(in Span<byte> data)
            => new GenericErrorMessage()
            {
                CommandType = data[0],
                ErrorCode = (ErrorCode)data[1],
            };

        public void Encode(PoweredUpMessage message, in Span<byte> data)
        => throw new NotImplementedException();
    }
}