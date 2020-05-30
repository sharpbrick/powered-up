namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.20.1
    public abstract class PortModeInformationMessage : CommonMessageHeader
    {
        public byte PortId { get; set; }
        public byte Mode { get; set; }
        public PortModeInformationType InformationType { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForNameMessage : PortModeInformationMessage
    {
        public string Name { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForRawMessage : PortModeInformationMessage
    {
        public float RawMin { get; set; }
        public float RawMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForPctMessage : PortModeInformationMessage
    {
        public float PctMin { get; set; }
        public float PctMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForSiMessage : PortModeInformationMessage
    {
        public float SiMin { get; set; }
        public float SiMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForSymbolMessage : PortModeInformationMessage
    {
        public string Symbol { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForMappingMessage : PortModeInformationMessage
    {
        public ushort Mapping { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForMotorBiasMessage : PortModeInformationMessage
    {
        public byte MotorBias { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForCapabilityBitsMessage : PortModeInformationMessage
    {
        public byte[] CapabilityBits { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForValueFormatMessage : PortModeInformationMessage
    {
        public byte[] ValueFormat { get; set; }
    }
}