namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.15.1
    public record PortInformationRequestMessage(byte PortId, PortInformationType InformationType) : LegoWirelessMessage(MessageType.PortInformationRequest);
}