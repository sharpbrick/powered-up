namespace SharpBrick.PoweredUp.Messages
{
    // spec chapter: 3.6.1
    public class HubActionMessage : CommonMessageHeader
    {
        public HubAction Action { get; set; }
    }
}