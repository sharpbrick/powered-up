namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.19.1
    public abstract class PortInformationMessage : PoweredUpMessage
    {
        public byte PortId { get; set; }
        public PortInformationType InformationType { get; set; }
    }

    // spec chapter: 3.19.1
    public class PortInformationForModeInfoMessage : PortInformationMessage
    {
        public bool OutputCapability { get; set; }
        public bool InputCapability { get; set; }
        public bool LogicalCombinableCapability { get; set; }
        public bool LogicalSynchronizableCapability { get; set; }

        public byte TotalModeCount { get; set; }
        public ushort InputModes { get; set; }
        public ushort OutputModes { get; set; }
    }

    // spec chapter: 3.19.1
    public class PortInformationForPossibleModeCombinationsMessage : PortInformationMessage
    {
        public ushort[] ModeCombinations { get; set; }
    }
}