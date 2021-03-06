namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.6.1
    public record HubActionMessage(HubAction Action) : LegoWirelessMessage(MessageType.HubActions)
    {
        public override string ToString()
            => $"Hub Action - {this.Action}";
    }
}