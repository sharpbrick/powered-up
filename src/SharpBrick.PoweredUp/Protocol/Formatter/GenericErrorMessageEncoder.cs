using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class GenericErrorMessageEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(CommonMessageHeader message)
            => throw new NotImplementedException();

        public CommonMessageHeader Decode(in Span<byte> data)
            => new GenericErrorMessage()
            {
                CommandType = data[0],
                ErrorCode = (ErrorCodes)data[1],
            };

        public void Encode(CommonMessageHeader message, in Span<byte> data)
        => throw new NotImplementedException();
    }
}