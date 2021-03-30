using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{

    // spec chapter: 3.15.1
    public record PortInformationRequestMessage(byte PortId, PortInformationType InformationType)
        : LegoWirelessMessage(MessageType.PortInformationRequest);

    // spec chapter: 3.16.1
    public record PortModeInformationRequestMessage(byte PortId, byte Mode, PortModeInformationType InformationType)
        : LegoWirelessMessage(MessageType.PortModeInformationRequest);

    // spec chapter: 3.19.1
    public abstract record PortInformationMessage(byte PortId, PortInformationType InformationType)
        : LegoWirelessMessage(MessageType.PortInformation);

    // spec chapter: 3.19.1
    public record PortInformationForModeInfoMessage(
        byte PortId,
        PortInformationType InformationType,
        bool OutputCapability,
        bool InputCapability,
        bool LogicalCombinableCapability,
        bool LogicalSynchronizableCapability,
        byte TotalModeCount,
        ushort InputModes,
        ushort OutputModes
        ) : PortInformationMessage(PortId, InformationType)
    {
        public override string ToString()
            => $"Port Information - Port {HubId}/{PortId} Total Modes {TotalModeCount} / Capabilities Output:{OutputCapability}, Input:{InputCapability}, LogicalCombinable:{LogicalCombinableCapability}, LogicalSynchronizable:{LogicalSynchronizableCapability} / InputModes: {InputModes:X}, OutputModes: {InputModes:X}";
    }

    // spec chapter: 3.19.1
    public record PortInformationForPossibleModeCombinationsMessage(byte PortId, PortInformationType InformationType, ushort[] ModeCombinations)
        : PortInformationMessage(PortId, InformationType)
    {
        public override string ToString()
            => $"Port Information (Combinations) - Port {HubId}/{PortId} Combinations: {string.Join(",", ModeCombinations.Select(x => x.ToString("X")))}";
    }

    // spec chapter: 3.20.1
    public abstract record PortModeInformationMessage(byte PortId, byte Mode, PortModeInformationType InformationType)
        : LegoWirelessMessage(MessageType.PortModeInformation);

    // spec chapter: 3.20.1
    public record PortModeInformationForNameMessage(byte PortId, byte Mode, string Name)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.Name);

    // spec chapter: 3.20.1
    public record PortModeInformationForRawMessage(byte PortId, byte Mode, float RawMin, float RawMax)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.Raw);

    // spec chapter: 3.20.1
    public record PortModeInformationForPctMessage(byte PortId, byte Mode, float PctMin, float PctMax)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.Pct);

    // spec chapter: 3.20.1
    public record PortModeInformationForSIMessage(byte PortId, byte Mode, float SIMin, float SIMax)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.SI);

    // spec chapter: 3.20.1
    public record PortModeInformationForSymbolMessage(byte PortId, byte Mode, string Symbol)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.Symbol);

    // spec chapter: 3.20.1
    public record PortModeInformationForMappingMessage(
            byte PortId, byte Mode,
            bool InputSupportsNull,
            bool InputSupportFunctionalMapping20,
            bool InputAbsolute,
            bool InputRelative,
            bool InputDiscrete,
            bool OutputSupportsNull,
            bool OutputSupportFunctionalMapping20,
            bool OutputAbsolute,
            bool OutputRelative,
            bool OutputDiscrete
        ) : PortModeInformationMessage(PortId, Mode, PortModeInformationType.Mapping);

    // spec chapter: 3.20.1
    public record PortModeInformationForMotorBiasMessage(byte PortId, byte Mode, byte MotorBias)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.MotorBias);

    // spec chapter: 3.20.1
    public record PortModeInformationForCapabilityBitsMessage(byte PortId, byte Mode, byte[] CapabilityBits)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.CapabilityBits);

    // spec chapter: 3.20.1
    public record PortModeInformationForValueFormatMessage(byte PortId, byte Mode, byte NumberOfDatasets, PortModeInformationDataType DatasetType, byte TotalFigures, byte Decimals)
        : PortModeInformationMessage(PortId, Mode, PortModeInformationType.ValueFormat);

}