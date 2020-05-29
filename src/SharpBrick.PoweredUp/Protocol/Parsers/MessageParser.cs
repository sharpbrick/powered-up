using System;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Parsers
{
    public class MessageParser
    {
        public static CommonMessageHeader Decode(in Span<byte> data)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderEncoder.ParseCommonHeader(data);

            var content = data.Slice(headerLength);

            return (MessageType)messageType switch
            {
                MessageType.HubProperties => HubPropertiesEncoder.Decode(content),
                MessageType.HubAttachedIO => HubAttachedIOEncoder.Decode(content),
                _ => new UnknownMessage(),
            };
        }
    }
}