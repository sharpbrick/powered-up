using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.WinRT;

namespace SharpBrick.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .ClearProviders()
            );

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            ulong bluetoothAddress = FindAndSelectHub(poweredUpBluetoothAdapter);

            if (bluetoothAddress == 0)
                return;

            using (var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()))
            {
                var protocol = new PoweredUpProtocol(kernel);

                await kernel.ConnectAsync();

                Console.WriteLine("Discover Ports. Reciving Messages ...");

                var discoverPorts = new DiscoverPorts(protocol);

                await protocol.ReceiveMessageAsync(message =>
                {
                    Console.Write(".");

                    return Task.CompletedTask;
                });

                await discoverPorts.ExecuteAsync();

                await protocol.SendMessageAsync(new HubActionMessage() { Action = HubAction.SwitchOffHub });

                Console.WriteLine(string.Empty);

                Console.WriteLine($"Discover Ports Function: {discoverPorts.ReceivedMessages} / {discoverPorts.SentMessages}");

                PrettyPrintKnowledge(System.Console.Out, protocol.Knowledge);
            }
        }

        private static ulong FindAndSelectHub(WinRTPoweredUpBluetoothAdapter poweredUpBluetoothAdapter)
        {
            ulong result = 0;
            var devices = new ConcurrentBag<(int key, ulong bluetoothAddresss, PoweredUpManufacturerDataConstants deviceType)>();
            var cts = new CancellationTokenSource();
            int idx = 1;

            Console.WriteLine("Scan Started. Please select the Hub (using a number keys or 'q' to terminate):");

            poweredUpBluetoothAdapter.Discover(info =>
            {
                if (devices.FirstOrDefault(kv => kv.bluetoothAddresss == info.BluetoothAddress) == default)
                {
                    var deviceType = (PoweredUpManufacturerDataConstants)info.ManufacturerData[1];
                    devices.Add((idx, info.BluetoothAddress, deviceType));

                    Console.WriteLine($"{idx}: {(PoweredUpManufacturerDataConstants)info.ManufacturerData[1]} (with address {info.BluetoothAddress})");

                    idx++;
                }
            }, cts.Token);

            var input = Console.ReadLine();

            cts.Cancel();

            if (int.TryParse(input, out var key))
            {
                var selected = devices.FirstOrDefault(kv => kv.key == key);

                result = selected.bluetoothAddresss; // default is 0

                if (result != default)
                {
                    Console.WriteLine($"Selected {selected.deviceType} with key {selected.key}");
                }
            }

            return result;
        }

        private static void PrettyPrintKnowledge(TextWriter writer, ProtocolKnowledge portKnowledge)
        {
            string Intent(int depth) => "                        ".Substring(0, depth * 2);

            foreach (var port in portKnowledge.Ports.Values.OrderBy(p => p.PortId))
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
                //TODO ModeCombinations { get; set; }


                writer.WriteLine($"{Intent(2)}UsedCombinationIndex: {port.UsedCombinationIndex}");
                writer.WriteLine($"{Intent(2)}MultiUpdateEnabled: {port.MultiUpdateEnabled}");
                writer.WriteLine($"{Intent(2)}ConfiguredModeDataSetIndex: [{string.Join(",", port.ConfiguredModeDataSetIndex)}]");

                foreach (var mode in port.Modes)
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
