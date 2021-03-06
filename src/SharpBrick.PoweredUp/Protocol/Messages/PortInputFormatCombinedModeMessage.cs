namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortInputFormatCombinedModeMessage(byte PortId, byte UsedCombinationIndex, bool MultiUpdateEnabled, int[] ConfiguredModeDataSetIndex) : LegoWirelessMessage(MessageType.PortInputFormatCombinedMode)
    {
        public override string ToString()
            => $"Port Input Format (Combined Mode) - Port {HubId}/{PortId} UsedCombinationIndex {UsedCombinationIndex} Enabled {MultiUpdateEnabled} Confirmed Modes/DataSet Indices {string.Join(",", ConfiguredModeDataSetIndex)}";
    }
}