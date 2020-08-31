namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.20.1
    public abstract class PortModeInformationMessage : LegoWirelessMessage
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
    public class PortModeInformationForSIMessage : PortModeInformationMessage
    {
        public float SIMin { get; set; }
        public float SIMax { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForSymbolMessage : PortModeInformationMessage
    {
        public string Symbol { get; set; }
    }

    // spec chapter: 3.20.1
    public class PortModeInformationForMappingMessage : PortModeInformationMessage
    {
        public bool InputSupportsNull { get; set; }
        public bool InputSupportFunctionalMapping20 { get; set; }
        public bool InputAbsolute { get; set; }
        public bool InputRelative { get; set; }
        public bool InputDiscrete { get; set; }

        public bool OutputSupportsNull { get; set; }
        public bool OutputSupportFunctionalMapping20 { get; set; }
        public bool OutputAbsolute { get; set; }
        public bool OutputRelative { get; set; }
        public bool OutputDiscrete { get; set; }
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
        public byte NumberOfDatasets { get; set; }
        public PortModeInformationDataType DatasetType { get; set; }
        public byte TotalFigures { get; set; }
        public byte Decimals { get; set; }
    }
}