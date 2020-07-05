using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAlertEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
        {
            var hubAlertMessage = message as HubAlertMessage ?? throw new ArgumentException(nameof(message));
            return (ushort)(hubAlertMessage.Operation == HubAlertOperation.Update ? 3 : 2);
        }

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
            => new HubAlertMessage() { Alert = (HubAlert)data[0], Operation = (HubAlertOperation)data[1], DownstreamPayload = (byte)((data[1] == (byte)HubAlertOperation.Update) ? data[2] : 0) };

        public void Encode(PoweredUpMessage message, in Span<byte> data)
        {
            var hubAlertMessage = message as HubAlertMessage ?? throw new ArgumentException(nameof(message));

            data[0] = (byte)hubAlertMessage.Alert;
            data[1] = (byte)hubAlertMessage.Operation;

            if (hubAlertMessage.Operation == HubAlertOperation.Update)
            {
                data[2] = hubAlertMessage.DownstreamPayload; // TODO: this name/direction is surely a spec issue
            }
        }
    }
}