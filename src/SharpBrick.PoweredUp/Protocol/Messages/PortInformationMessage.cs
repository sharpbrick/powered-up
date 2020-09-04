using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.19.1
    public abstract class PortInformationMessage : LegoWirelessMessage
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

        public override string ToString()
            => $"Port Information - Port {HubId}/{PortId} Total Modes {TotalModeCount} / Capabilities Output:{OutputCapability}, Input:{InputCapability}, LogicalCombinable:{LogicalCombinableCapability}, LogicalSynchronizable:{LogicalSynchronizableCapability} / InputModes: {InputModes:X}, OutputModes: {InputModes:X}";
    }

    // spec chapter: 3.19.1
    public class PortInformationForPossibleModeCombinationsMessage : PortInformationMessage
    {
        public ushort[] ModeCombinations { get; set; }

        public override string ToString()
            => $"Port Information (Combinations) - Port {HubId}/{PortId} Combinations: {string.Join(",", ModeCombinations.Select(x => x.ToString("X")))}";
    }
}