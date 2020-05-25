namespace SharpBrick.PoweredUp.Messages
{
    // spec chapter: 3.7.1
    public class HubAlertMessage : CommonMessageHeader
    {
        public HubAlert Alert { get; set; }
        public HubAlertOperation Operation { get; set; }

        public byte DownstreamPayload { get; set; }
    }
}