using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class HubAlertEncoder : IMessageContentEncoder
{
    public ushort CalculateContentLength(LegoWirelessMessage message)
    {
        var hubAlertMessage = message as HubAlertMessage ?? throw new ArgumentException("message is null or not HubAlertMessage", nameof(message));
        return (ushort)(hubAlertMessage.Operation == HubAlertOperation.Update ? 3 : 2);
    }

    public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        => new HubAlertMessage((HubAlert)data[0], (HubAlertOperation)data[1], (byte)((data[1] == (byte)HubAlertOperation.Update) ? data[2] : 0));

    public void Encode(LegoWirelessMessage message, in Span<byte> data)
    {
        var hubAlertMessage = message as HubAlertMessage ?? throw new ArgumentException("message is null or not HubAlertMessage", nameof(message));

        data[0] = (byte)hubAlertMessage.Alert;
        data[1] = (byte)hubAlertMessage.Operation;

        if (hubAlertMessage.Operation == HubAlertOperation.Update)
        {
            data[2] = hubAlertMessage.DownstreamPayload; // TODO: this name/direction is surely a spec issue
        }
    }
}
