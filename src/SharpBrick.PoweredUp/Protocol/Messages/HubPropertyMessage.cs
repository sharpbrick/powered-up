using System;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.5.2

    public record HubPropertyMessage(HubProperty Property, HubPropertyOperation Operation) : LegoWirelessMessage(MessageType.HubProperties);
    public record HubPropertyMessage<TPayload>(HubProperty Property, HubPropertyOperation Operation, TPayload Payload) : HubPropertyMessage(Property, Operation)
    {
        public override string ToString()
            => this switch
            {
                HubPropertyMessage<byte[]> msg => $"Hub Property - {msg.Property}: {BytesStringUtil.DataToString(msg.Payload)}",
                _ => $"Hub Property - {this.Property}: {this.Payload}",
            };
    }
}