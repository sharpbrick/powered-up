using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class VirtualPortSetupEncoder : IMessageContentEncoder
{
    public ushort CalculateContentLength(LegoWirelessMessage message)
        => message switch
        {
            VirtualPortSetupForConnectedMessage => 3,
            VirtualPortSetupForDisconnectedMessage => 2,
            _ => throw new NotSupportedException(),
        };

    public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        => throw new NotImplementedException();

    public void Encode(LegoWirelessMessage message, in Span<byte> data)
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
