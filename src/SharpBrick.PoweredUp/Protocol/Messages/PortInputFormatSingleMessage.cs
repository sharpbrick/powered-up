namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.23
    public record PortInputFormatSingleMessage(byte PortId, byte ModeIndex, uint DeltaInterval, bool NotificationEnabled) : LegoWirelessMessage(MessageType.PortInputFormatSingle)
    {
        public override string ToString()
            => $"Port Input Format (Single) - Mode {HubId}/{PortId}/{ModeIndex}: Threshold {DeltaInterval}, Notification {NotificationEnabled}";
    }
}