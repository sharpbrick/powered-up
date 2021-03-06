namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record VirtualPortSetupMessage(VirtualPortSubCommand SubCommand) : LegoWirelessMessage(MessageType.VirtualPortSetup);

    public record VirtualPortSetupForDisconnectedMessage(byte PortId) : VirtualPortSetupMessage(VirtualPortSubCommand.Disconnected);

    public record VirtualPortSetupForConnectedMessage(byte PortAId, byte PortBId) : VirtualPortSetupMessage(VirtualPortSubCommand.Connected);
}