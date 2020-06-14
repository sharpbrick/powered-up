using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortValueSingleMessage : PoweredUpMessage
    {
        public PortValueData[] Data { get; set; }

        public static string FormatPortValueDataArray(byte hubId, in PortValueData[] data)
            => string.Join(";", data.Select(d => d switch
                {
                    PortValueData<sbyte> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<short> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<int> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<float> dd => $"Mode {hubId}/{dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    _ => "Undefined Data Type",
                }));

        public override string ToString()
            => "Port Values - " + FormatPortValueDataArray(HubId, Data);
    }
}