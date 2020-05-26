namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.19.1
    public abstract class PortInformationCommonMessageHeader : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte InformationType { get; set; }
    }

    // spec chapter: 3.19.1
    public class PortInformationForModeInfoMessage : PortInformationCommonMessageHeader
    {
        public byte Capabilities { get; set; }
        public byte TotalModeCount { get; set; }
        public ushort InputModes { get; set; }
        public ushort OutputModes { get; set; }
    }

    // spec chapter: 3.19.1
    public class PortInformationForPossibleModeCombinationsMessage : PortInformationCommonMessageHeader
    {
        public ushort[] ModeCombinations { get; set; }
    }
}