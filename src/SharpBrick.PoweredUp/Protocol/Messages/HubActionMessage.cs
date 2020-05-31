namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.6.1
    public class HubActionMessage : PoweredUpMessage
    {
        public HubAction Action { get; set; }
    }
}