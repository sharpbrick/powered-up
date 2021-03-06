using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.19.1
    public abstract record PortInformationMessage(byte PortId, PortInformationType InformationType) : LegoWirelessMessage(MessageType.PortInformation);

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
    public record PortInformationForPossibleModeCombinationsMessage(byte PortId, PortInformationType InformationType, ushort[] ModeCombinations) : PortInformationMessage(PortId, InformationType)
    {
        public override string ToString()
            => $"Port Information (Combinations) - Port {HubId}/{PortId} Combinations: {string.Join(",", ModeCombinations.Select(x => x.ToString("X")))}";
    }
}