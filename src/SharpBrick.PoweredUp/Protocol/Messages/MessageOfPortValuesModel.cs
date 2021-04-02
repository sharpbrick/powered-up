using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortValueSingleMessage(PortValueData[] Data)
        : LegoWirelessMessage(MessageType.PortValueSingle)
    {
        public static string FormatPortValueDataArray(byte hubId, in PortValueData[] data)
            => string.Join(";", data.Select(d => d switch
                {
                    PortValueData<sbyte, sbyte> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<short, short> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<int, int> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<float, float> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<sbyte, double> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<short, double> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<int, double> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<float, double> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    _ => "Undefined Data Type",
                }));

        public override string ToString()
            => "Port Values - " + FormatPortValueDataArray(HubId, Data);
    }
    public record PortValueCombinedModeMessage(byte PortId, PortValueData[] Data)
        : LegoWirelessMessage(MessageType.PortValueCombinedMode)
    {
        public override string ToString()
            => $"Port Values (Combined Mode) - " + PortValueSingleMessage.FormatPortValueDataArray(HubId, Data);
    }
}