namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortInputFormatCombinedModeMessage : LegoWirelessMessage
    {
        public byte PortId { get; set; }
        public byte UsedCombinationIndex { get; set; }
        public bool MultiUpdateEnabled { get; set; }
        public int[] ConfiguredModeDataSetIndex { get; set; }

        public override string ToString()
            => $"Port Input Format (Combined Mode) - Port {HubId}/{PortId} UsedCombinationIndex {UsedCombinationIndex} Enabled {MultiUpdateEnabled} Confirmed Modes/DataSet Indices {string.Join(",", ConfiguredModeDataSetIndex)}";
    }
}