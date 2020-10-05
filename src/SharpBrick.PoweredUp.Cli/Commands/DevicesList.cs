using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Cli
{
    public class DevicesList
    {
        private readonly ILegoWirelessProtocol protocol;
        private readonly DiscoverPorts discoverPorts;

        public DevicesList(ILegoWirelessProtocol protocol, DiscoverPorts discoverPorts)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.discoverPorts = discoverPorts ?? throw new ArgumentNullException(nameof(discoverPorts));
        }
        public async Task ExecuteAsync(SystemType knownSystemType)
        {
            Console.WriteLine("Discover Ports. Receiving Messages ...");

            await protocol.ConnectAsync(knownSystemType); // registering to bluetooth notification

            await Task.Delay(2000); // await ports to be announced initially by device.

            using var disposable = protocol.UpstreamMessages.Subscribe(x => Console.Write("."));

            await discoverPorts.ExecuteAsync();

            await protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage() { HubId = 0, Action = HubAction.SwitchOffHub }, result => result.Action == HubAction.HubWillSwitchOff);

            Console.WriteLine(string.Empty);

            Console.WriteLine($"Discover Ports Function: {discoverPorts.ReceivedMessages} / {discoverPorts.SentMessages}");

            PrettyPrintKnowledge(System.Console.Out, protocol.Knowledge);
        }

        private static void PrettyPrintKnowledge(TextWriter writer, ProtocolKnowledge portKnowledge)
        {
            string Intent(int depth) => "                        ".Substring(0, depth * 2);

            foreach (var hub in portKnowledge.Hubs.OrderBy(h => h.HubId))
            {
                Console.WriteLine($"{Intent(0)}Hub: {hub.HubId}");
                foreach (var port in hub.Ports.Values.OrderBy(p => p.PortId))
                {
                    writer.WriteLine($"{Intent(1)}Port: {port.PortId}");
                    writer.WriteLine($"{Intent(2)}IOTypeId: {port.IOTypeId}");
                    writer.WriteLine($"{Intent(2)}HardwareRevision: {port.HardwareRevision}");
                    writer.WriteLine($"{Intent(2)}SoftwareRevision: {port.SoftwareRevision}");

                    writer.WriteLine($"{Intent(2)}OutputCapability: {port.OutputCapability}");
                    writer.WriteLine($"{Intent(2)}InputCapability: {port.InputCapability}");
                    writer.WriteLine($"{Intent(2)}LogicalCombinableCapability: {port.LogicalCombinableCapability}");
                    writer.WriteLine($"{Intent(2)}LogicalSynchronizableCapability: {port.LogicalSynchronizableCapability}");

                    // PortInformationForPossibleModeCombinationsMessage
                    writer.WriteLine($"{Intent(2)}ModeCombinations: [{string.Join(", ", port.ModeCombinations.Select(x => BytesStringUtil.ToBitString(x)))}]");

                    writer.WriteLine($"{Intent(2)}UsedCombinationIndex: {port.UsedCombinationIndex}");
                    writer.WriteLine($"{Intent(2)}MultiUpdateEnabled: {port.MultiUpdateEnabled}");
                    writer.WriteLine($"{Intent(2)}ConfiguredModeDataSetIndex: [{string.Join(",", port.ConfiguredModeDataSetIndex)}]");

                    foreach (var mode in port.Modes.Values.OrderBy(m => m.ModeIndex))
                    {
                        writer.WriteLine($"{Intent(2)}Mode: {mode.ModeIndex}");
                        writer.WriteLine($"{Intent(3)}Name: {mode.Name}");
                        writer.WriteLine($"{Intent(3)}IsInput: {mode.IsInput}");
                        writer.WriteLine($"{Intent(3)}IsOutput: {mode.IsOutput}");

                        writer.WriteLine($"{Intent(3)}RawMin: {mode.RawMin}");
                        writer.WriteLine($"{Intent(3)}RawMax: {mode.RawMax}");

                        // PortModeInformationForPctMessage
                        writer.WriteLine($"{Intent(3)}PctMin: {mode.PctMin}");
                        writer.WriteLine($"{Intent(3)}PctMax: {mode.PctMax}");

                        // PortModeInformationForSIMessage
                        writer.WriteLine($"{Intent(3)}SIMin: {mode.SIMin}");
                        writer.WriteLine($"{Intent(3)}SIMax: {mode.SIMax}");

                        // PortModeInformationForSymbolMessage
                        writer.WriteLine($"{Intent(3)}Symbol: {mode.Symbol}");

                        // PortModeInformationForMappingMessage
                        writer.WriteLine($"{Intent(3)}InputSupportsNull: {mode.InputSupportsNull}");
                        writer.WriteLine($"{Intent(3)}InputSupportFunctionalMapping20: {mode.InputSupportFunctionalMapping20}");
                        writer.WriteLine($"{Intent(3)}InputAbsolute: {mode.InputAbsolute}");
                        writer.WriteLine($"{Intent(3)}InputRelative: {mode.InputRelative}");
                        writer.WriteLine($"{Intent(3)}InputDiscrete: {mode.InputDiscrete}");

                        writer.WriteLine($"{Intent(3)}OutputSupportsNull: {mode.OutputSupportsNull}");
                        writer.WriteLine($"{Intent(3)}OutputSupportFunctionalMapping20: {mode.OutputSupportFunctionalMapping20}");
                        writer.WriteLine($"{Intent(3)}OutputAbsolute: {mode.OutputAbsolute}");
                        writer.WriteLine($"{Intent(3)}OutputRelative: {mode.OutputRelative}");
                        writer.WriteLine($"{Intent(3)}OutputDiscrete: {mode.OutputDiscrete}");

                        // PortModeInformationForValueFormatMessage
                        writer.WriteLine($"{Intent(3)}NumberOfDatasets: {mode.NumberOfDatasets}");
                        writer.WriteLine($"{Intent(3)}DatasetType: {mode.DatasetType}");
                        writer.WriteLine($"{Intent(3)}TotalFigures: {mode.TotalFigures}");
                        writer.WriteLine($"{Intent(3)}Decimals: {mode.Decimals}");

                        // PortInputFormatSingleMessage
                        writer.WriteLine($"{Intent(3)}DeltaInterval: {mode.DeltaInterval}");
                        writer.WriteLine($"{Intent(3)}NotificationEnabled: {mode.NotificationEnabled}");
                    }
                }
            }
        }
    }
}
