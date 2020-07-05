using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInformationRequestEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => 2;

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => Encode(message as PortInformationRequestMessage ?? throw new ArgumentException(nameof(message)), data);

        public void Encode(PortInformationRequestMessage message, in Span<byte> data)
        {
            data[0] = (byte)message.PortId;
            data[1] = (byte)message.InformationType;
        }
    }
}