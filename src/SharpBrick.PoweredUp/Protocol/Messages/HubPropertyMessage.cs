namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.5.2

    public class HubPropertyMessage : CommonMessageHeader
    {
        public HubProperty Property { get; set; }
        public HubPropertyOperation Operation { get; set; }
    }
    public class HubPropertyMessage<TPayload> : HubPropertyMessage
    {
        public TPayload Payload { get; set; }
    }
}