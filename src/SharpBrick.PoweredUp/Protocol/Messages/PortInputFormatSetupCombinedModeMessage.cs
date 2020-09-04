namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortInputFormatSetupCombinedModeMessage : LegoWirelessMessage
    {
        public byte PortId { get; set; }
        public PortInputFormatSetupCombinedSubCommand SubCommand { get; set; }
    }

    public class PortInputFormatSetupCombinedModeForSetModeDataSetMessage : PortInputFormatSetupCombinedModeMessage
    {
        public byte CombinationIndex { get; set; }
        public PortInputFormatSetupCombinedModeModeDataSet[] ModeDataSets { get; set; }
    }
}