using System;

namespace SharpBrick.PoweredUp.Messages
{
    // spec chapter: 3.1
    public class CommonMessageHeader
    {
        // spec chapter: 3.2
        // encoded as byte or ushort
        public ushort Length { get; set; }
        public byte HubId { get; set; }
        public byte MessageType { get; set; }
    }
}
