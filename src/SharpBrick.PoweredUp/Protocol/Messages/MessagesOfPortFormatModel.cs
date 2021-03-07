namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortInputFormatCombinedModeMessage(byte PortId, byte UsedCombinationIndex, bool MultiUpdateEnabled, int[] ConfiguredModeDataSetIndex)
        : LegoWirelessMessage(MessageType.PortInputFormatCombinedMode)
    {
        public override string ToString()
            => $"Port Input Format (Combined Mode) - Port {HubId}/{PortId} UsedCombinationIndex {UsedCombinationIndex} Enabled {MultiUpdateEnabled} Confirmed Modes/DataSet Indices {string.Join(",", ConfiguredModeDataSetIndex)}";
    }
    public record PortInputFormatSetupCombinedModeMessage(byte PortId, PortInputFormatSetupCombinedSubCommand SubCommand)
        : LegoWirelessMessage(MessageType.PortInputFormatSetupCombinedMode);

    public record PortInputFormatSetupCombinedModeForSetModeDataSetMessage(byte PortId, byte CombinationIndex, PortInputFormatSetupCombinedModeModeDataSet[] ModeDataSets)
        : PortInputFormatSetupCombinedModeMessage(PortId, PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination);

    // spec chapter: 3.17
    public record PortInputFormatSetupSingleMessage(byte PortId, byte Mode, uint DeltaInterval, bool NotificationEnabled)
        : LegoWirelessMessage(MessageType.PortInputFormatSetupSingle);

    // spec chapter: 3.23
    public record PortInputFormatSingleMessage(byte PortId, byte ModeIndex, uint DeltaInterval, bool NotificationEnabled)
        : LegoWirelessMessage(MessageType.PortInputFormatSingle)
    {
        public override string ToString()
            => $"Port Input Format (Single) - Mode {HubId}/{PortId}/{ModeIndex}: Threshold {DeltaInterval}, Notification {NotificationEnabled}";
    }
}