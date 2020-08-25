using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    // spec chapter: 3.17
    public class PortInputFormatSetupSingleEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => 7;

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => Encode(message as PortInputFormatSetupSingleMessage ?? throw new ArgumentException(nameof(message)), data);
        public void Encode(PortInputFormatSetupSingleMessage message, in Span<byte> data)
        {
            data[0] = message.PortId;
            data[1] = message.Mode;
            BitConverter.TryWriteBytes(data.Slice(2, 4), message.DeltaInterval);
            data[6] = (byte)(message.NotificationEnabled ? 0x01 : 0x00);
        }
    }
}