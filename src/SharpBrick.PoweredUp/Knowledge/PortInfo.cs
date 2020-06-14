using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Knowledge
{
    public class PortInfo
    {
        public byte PortId { get; set; }

        // HubAttachedIOForAttachedDeviceMessage
        public bool IsDeviceConnected { get; set; } = false;
        public DeviceType IOTypeId { get; set; }
        public Version HardwareRevision { get; set; }
        public Version SoftwareRevision { get; set; }

        // PortInformationForModeInfoMessage
        public bool OutputCapability { get; set; }
        public bool InputCapability { get; set; }
        public bool LogicalCombinableCapability { get; set; }
        public bool LogicalSynchronizableCapability { get; set; }
        public PortModeInfo[] Modes { get; set; }

        // PortInformationForPossibleModeCombinationsMessage
        public ushort[] ModeCombinations { get; set; }

        // PortInputFormatCombinedModeMessage
        public byte UsedCombinationIndex { get; set; }
        public bool MultiUpdateEnabled { get; set; } = false;
        public int[] ConfiguredModeDataSetIndex { get; set; } = Array.Empty<int>();

        // PortInputFormatSetupCombinedModeForSetModeDataSetMessage
        public PortInputFormatSetupCombinedModeModeDataSet[] RequestedCombinedModeDataSets { get; internal set; }


        public byte LastFormattedPortMode { get; internal set; }
    }
}