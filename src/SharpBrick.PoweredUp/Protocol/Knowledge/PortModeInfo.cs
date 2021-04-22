using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Knowledge
{
    public class PortModeInfo
    {
        public byte HubId { get; set; }
        public byte PortId { get; set; }
        public byte ModeIndex { get; set; }

        // PortInformationForModeInfoMessage
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }

        // PortModeInformationForNameMessage
        public string Name { get; set; }

        // PortModeInformationForRawMessage
        public float RawMin { get; set; }
        public float RawMax { get; set; }

        // PortModeInformationForPctMessage
        public float PctMin { get; set; }
        public float PctMax { get; set; }

        // PortModeInformationForSIMessage
        public float SIMin { get; set; }
        public float SIMax { get; set; }

        // PortModeInformationForSymbolMessage
        public string Symbol { get; set; }

        // PortModeInformationForMappingMessage
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

        // PortModeInformationForValueFormatMessage
        public byte NumberOfDatasets { get; set; }
        public PortModeInformationDataType DatasetType { get; set; }
        public byte TotalFigures { get; set; }
        public byte Decimals { get; set; }

        // PortInputFormatSingleMessage
        public uint DeltaInterval { get; set; }
        public bool NotificationEnabled { get; set; }


        // Additional Settings
        public bool DisablePercentage { get; set; } = false;
        public bool DisableScaling { get; set; } = false;
        public bool OverrideDatasetTypeToDouble { get; set; } = false;
    }
}