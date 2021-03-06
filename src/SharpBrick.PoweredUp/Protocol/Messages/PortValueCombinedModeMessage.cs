namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortValueCombinedModeMessage(byte PortId, PortValueData[] Data) : LegoWirelessMessage(MessageType.PortValueCombinedMode)
    {
        public override string ToString()
            => $"Port Values (Combined Mode) - " + PortValueSingleMessage.FormatPortValueDataArray(HubId, Data);
    }
}