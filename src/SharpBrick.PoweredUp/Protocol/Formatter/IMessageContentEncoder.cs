using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public interface IMessageContentEncoder
    {
        ushort CalculateContentLength(LegoWirelessMessage message);
        void Encode(LegoWirelessMessage message, in Span<byte> data);
        LegoWirelessMessage Decode(byte hubId, in Span<byte> data);
    }
}