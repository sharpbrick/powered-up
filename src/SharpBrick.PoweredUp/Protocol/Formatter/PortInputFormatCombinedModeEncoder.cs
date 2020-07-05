using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInputFormatCombinedModeEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
        {
            ushort bitMask = BitConverter.ToUInt16(data.Slice(2, 2));

            return new PortInputFormatCombinedModeMessage()
            {
                PortId = data[0],
                UsedCombinationIndex = (byte)(data[1] & 0b0000_0111),
                MultiUpdateEnabled = (byte)(data[1] & 0b1000_0000) > 0,
                ConfiguredModeDataSetIndex = Enumerable.Range(0, 16).Select(pos =>
                    (bitMask & (0x01 << pos)) > 0 ? pos : -1
                ).Where(mdi => mdi >= 0).ToArray(),
            };
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();

    }

}