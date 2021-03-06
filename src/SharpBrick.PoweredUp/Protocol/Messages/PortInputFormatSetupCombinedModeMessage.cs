namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortInputFormatSetupCombinedModeMessage(byte PortId, PortInputFormatSetupCombinedSubCommand SubCommand) : LegoWirelessMessage(MessageType.PortInputFormatSetupCombinedMode);

    public record PortInputFormatSetupCombinedModeForSetModeDataSetMessage(byte PortId, byte CombinationIndex, PortInputFormatSetupCombinedModeModeDataSet[] ModeDataSets) : PortInputFormatSetupCombinedModeMessage(PortId, PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination);
}