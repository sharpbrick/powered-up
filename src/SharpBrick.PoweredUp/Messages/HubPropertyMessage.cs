namespace SharpBrick.PoweredUp.Messages
{
    // spec chapter: 3.5.2
    public class HubPropertyMessage : CommonMessageHeader
    {
        public HubProperty Property { get; set; }
        public HubPropertyOperation Operation { get; set; }

        public byte[] Payload { get; set; }
    }
}