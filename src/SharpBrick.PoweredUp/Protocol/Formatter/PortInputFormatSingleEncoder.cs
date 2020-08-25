using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    // spec chapter: 3.23
    public class PortInputFormatSingleEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => 7;

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => Encode(message as PortInputFormatSingleMessage, data);
        public void Encode(PortInputFormatSingleMessage message, in Span<byte> data)
        {
            data[0] = message.PortId;
            data[1] = message.ModeIndex;
            BitConverter.TryWriteBytes(data.Slice(2, 4), message.DeltaInterval);
            data[6] = (byte)(message.NotificationEnabled ? 0x01 : 0x00);
        }

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
            => new PortInputFormatSingleMessage()
            {
                PortId = data[0],
                ModeIndex = data[1],
                DeltaInterval = BitConverter.ToUInt32(data.Slice(2, 4)),
                NotificationEnabled = (data[6] == 0x01),
            };
    }
}