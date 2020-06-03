namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class VirtualPortSetupMessage : PoweredUpMessage
    {
        public VirtualPortSubCommand SubCommand { get; set; }
    }

    public class VirtualPortSetupForDisconnectedMessage : VirtualPortSetupMessage
    {
        public byte PortId { get; set; }
    }

    public class VirtualPortSetupForConnectedMessage : VirtualPortSetupMessage
    {
        public byte PortAId { get; set; }
        public byte PortBId { get; set; }
    }
}