using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubActionEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => 1;

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
            => new HubActionMessage((HubAction)data[0]);

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
        {
            var hubActionMessage = message as HubActionMessage ?? throw new ArgumentException(nameof(message));

            data[0] = (byte)hubActionMessage.Action;
        }
    }
}