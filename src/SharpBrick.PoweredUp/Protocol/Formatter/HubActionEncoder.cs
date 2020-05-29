using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubActionEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(CommonMessageHeader message)
            => 1;

        public CommonMessageHeader Decode(in Span<byte> data)
            => new HubActionMessage() { Action = (HubAction)data[0] };

        public void Encode(CommonMessageHeader message, in Span<byte> data)
        {
            var hubActionMessage = message as HubActionMessage ?? throw new ArgumentException(nameof(message));

            data[0] = (byte)hubActionMessage.Action;
        }
    }
}