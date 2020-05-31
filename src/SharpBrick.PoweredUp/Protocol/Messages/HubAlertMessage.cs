namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.7.1
    public class HubAlertMessage : PoweredUpMessage
    {
        public HubAlert Alert { get; set; }
        public HubAlertOperation Operation { get; set; }

        public byte DownstreamPayload { get; set; }
    }
}