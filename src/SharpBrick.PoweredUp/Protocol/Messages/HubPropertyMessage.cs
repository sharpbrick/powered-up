using System;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.5.2

    public class HubPropertyMessage : PoweredUpMessage
    {
        public HubProperty Property { get; set; }
        public HubPropertyOperation Operation { get; set; }
    }
    public class HubPropertyMessage<TPayload> : HubPropertyMessage
    {
        public TPayload Payload { get; set; }

        public override string ToString()
            => this switch
            {
                HubPropertyMessage<byte[]> msg => $"Hub Property - {msg.Property}: {BytesStringUtil.DataToString(msg.Payload)}",
                _ => $"Hub Property - {this.Property}: {this.Payload}",
            };
    }
}