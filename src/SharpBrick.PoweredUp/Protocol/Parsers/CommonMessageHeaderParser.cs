using System;

namespace SharpBrick.PoweredUp.Protocol.Parsers
{
    public static class CommonMessageHeaderParser
    {
        public static (ushort length, byte hubId, byte messageType, ushort headerLength) ParseCommonHeader(in Span<byte> data)
        {
            const byte LongLengthMessageBit = 0b1000_0000;
            bool longHeader = (data[0] & LongLengthMessageBit) == LongLengthMessageBit;

            ushort length = data[0];

            if (longHeader)
            {
                length = (ushort)((data[1] << 7) + (data[0] & 0b0111_1111));
            }

            var hubId = data[longHeader ? 2 : 1];
            var messageType = data[longHeader ? 3 : 2];
            var headerLength = (ushort)(longHeader ? 4 : 3);

            return (length, hubId, messageType, headerLength);
        }
    }
}