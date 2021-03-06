namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.16.1
    public record PortModeInformationRequestMessage(byte PortId, byte Mode, PortModeInformationType InformationType) : LegoWirelessMessage(MessageType.PortModeInformationRequest);
}