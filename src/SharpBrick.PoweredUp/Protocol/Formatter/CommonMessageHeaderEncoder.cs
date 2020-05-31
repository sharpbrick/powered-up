using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class CommonMessageHeaderEncoder
    {
        public static void DecodeAndApply(in Span<byte> data, PoweredUpMessage message)
        {
            var (length, hubId, messageType, headerLength) = ParseCommonHeader(data);

            message.Length = length;
            message.HubId = hubId;
            message.MessageType = (MessageType)messageType;
        }

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

        public static void Encode(ushort contentLength, MessageType messageType, Span<byte> data)
        {
            int offset = 0;

            if (contentLength + 3 > 127)
            {
                var length = (ushort)(contentLength + 4);
                offset = 1;

                data[0] = (byte)((length & 0b0000_0000_0111_1111) | 0b1000_0000);
                data[1] = (byte)(length >> 7);
            }
            else
            {
                var length = (byte)(contentLength + 3);
                data[0] = length;
            }

            data[offset + 1] = 0x00;
            data[offset + 2] = (byte)messageType;
        }
    }
}