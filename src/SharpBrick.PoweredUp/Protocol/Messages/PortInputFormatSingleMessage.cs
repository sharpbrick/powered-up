namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.23
    public class PortInputFormatSingleMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public byte ModeIndex { get; set; }
        public uint DeltaInterval { get; set; }
        public bool NotificationEnabled { get; set; }

        public override string ToString()
            => $"Port Input Format (Single) - Mode {HubId}/{PortId}/{ModeIndex}: Threshold {DeltaInterval}, Notification {NotificationEnabled}";
    }
}