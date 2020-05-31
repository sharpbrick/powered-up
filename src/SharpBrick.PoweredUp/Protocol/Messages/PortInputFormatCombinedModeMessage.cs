namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortInputFormatCombinedModeMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public byte UsedCombinationIndex { get; set; }
        public bool MultiUpdateEnabled { get; set; }
        public int[] ConfiguredModeDataSetIndex { get; set; }
    }
}