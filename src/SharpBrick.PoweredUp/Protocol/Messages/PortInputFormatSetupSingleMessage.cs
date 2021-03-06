namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.17
    public record PortInputFormatSetupSingleMessage(byte PortId, byte Mode, uint DeltaInterval, bool NotificationEnabled) : LegoWirelessMessage(MessageType.PortInputFormatSetupSingle);
}