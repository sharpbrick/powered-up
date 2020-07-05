using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class VirtualPortSetupEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => message switch
            {
                VirtualPortSetupForConnectedMessage msg => 3,
                VirtualPortSetupForDisconnectedMessage msg => 2,
                _ => throw new NotSupportedException(),
            };

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(PoweredUpMessage message, in Span<byte> data)
        {
            switch (message)
            {
                case VirtualPortSetupForConnectedMessage msg:
                    data[0] = (byte)VirtualPortSubCommand.Connected;
                    data[1] = msg.PortAId;
                    data[2] = msg.PortBId;
                    break;
                case VirtualPortSetupForDisconnectedMessage msg:
                    data[0] = (byte)VirtualPortSubCommand.Disconnected;
                    data[1] = msg.PortId;
                    break;
            }
        }
    }
}