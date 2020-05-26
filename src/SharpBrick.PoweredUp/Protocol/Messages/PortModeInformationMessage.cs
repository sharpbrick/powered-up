namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.20.1
    public abstract class PortModeInformationCommonMessageHeader : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public byte InformationType { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForNameMessage : PortModeInformationCommonMessageHeader
    {
        public byte[] Name { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForRawMessage : PortModeInformationCommonMessageHeader
    {
        public float RawMin { get; set; }
        public float RawMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForPctMessage : PortModeInformationCommonMessageHeader
    {
        public float PctMin { get; set; }
        public float PctMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForSiMessage : PortModeInformationCommonMessageHeader
    {
        public float SiMin { get; set; }
        public float SiMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForSymbolMessage : PortModeInformationCommonMessageHeader
    {
        public byte[] Symbol { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForMappingMessage : PortModeInformationCommonMessageHeader
    {
        public ushort Mapping { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForMotorBiasMessage : PortModeInformationCommonMessageHeader
    {
        public byte MotorBias { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForCapabilityBitsMessage : PortModeInformationCommonMessageHeader
    {
        public byte[] CapabilityBits { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForValueFormatMessage : PortModeInformationCommonMessageHeader
    {
        public byte[] ValueFormat { get; set; }
    }
}