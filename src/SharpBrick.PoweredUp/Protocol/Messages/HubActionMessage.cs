namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.6.1
    public class HubActionMessage : LegoWirelessMessage
    {
        public HubAction Action { get; set; }

        public override string ToString()
            => $"Hub Action - {this.Action}";
    }
}