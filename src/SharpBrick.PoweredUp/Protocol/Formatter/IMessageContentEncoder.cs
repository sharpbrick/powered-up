using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public interface IMessageContentEncoder
    {
        ushort CalculateContentLength(PoweredUpMessage message);
        void Encode(PoweredUpMessage message, in Span<byte> data);
        PoweredUpMessage Decode(byte hubId, in Span<byte> data);
    }
}