using System;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.1
    // Length: spec chapter: 3.2; encoded as byte or ushort
    public abstract record LegoWirelessMessage(MessageType MessageType)
    {
        public ushort Length { get; set; }
        public byte HubId { get; set; }
    }
}
