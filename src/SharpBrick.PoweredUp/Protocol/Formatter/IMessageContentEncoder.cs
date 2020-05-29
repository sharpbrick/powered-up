using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public interface IMessageContentEncoder
    {
        ushort CalculateContentLength(CommonMessageHeader message);
        void Encode(CommonMessageHeader message, in Span<byte> data);
        CommonMessageHeader Decode(in Span<byte> data);
    }
}