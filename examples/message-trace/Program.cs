using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.WinRT;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Examples.MessageTrace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddConsole()
                    .AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug)
            );

            var logger = loggerFactory.CreateLogger("Main");

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            logger.LogInformation("Finding Service");
            ulong bluetoothAddress = 0;
            var cts = new CancellationTokenSource();
            poweredUpBluetoothAdapter.Discover(info =>
            {
                logger.LogInformation($"Found {info.BluetoothAddress} is a {(PoweredUpManufacturerDataConstants)info.ManufacturerData[1]}");

                bluetoothAddress = info.BluetoothAddress;
            }, cts.Token);

            logger.LogInformation("Press any key to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();

            // ulong bluetoothAddress = 158897336311065;

            if (bluetoothAddress == 0)
                return;

            using (var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()))
            {
                await kernel.ConnectAsync();

                await kernel.ReceiveBytesAsync(async data =>
                {
                    var message = MessageEncoder.Decode(data);

                    string messageAsString = message switch
                    {
                        HubPropertyMessage<Version> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<string> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<bool> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<sbyte> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<byte> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<byte[]> msg => $"Hub Property - {msg.Property}: {BytesStringUtil.DataToString(msg.Payload)}",
                        HubActionMessage msg => $"Hub Action - {msg.Action}",
                        HubAttachedIOForAttachedDeviceMessage msg => $"Attached IO - Port {msg.PortId} of type {msg.IOTypeId} (HW: {msg.HardwareRevision} / SW: {msg.SoftwareRevision})",
                        HubAttachedIOForDetachedDeviceMessage msg => $"Dettached IO - Port {msg.PortId}",
                        GenericErrorMessage msg => $"Error - {msg.ErrorCode} from {(MessageType)msg.CommandType}",
                        PortInformationForModeInfoMessage msg => $"Port Information - Port {msg.PortId} Total Modes {msg.TotalModeCount} / Capabilities Output:{msg.OutputCapability}, Input:{msg.InputCapability}, LogicalCombinable:{msg.LogicalCombinableCapability}, LogicalSynchronizable:{msg.LogicalSynchronizableCapability} / InputModes: {msg.InputModes:X}, OutputModes: {msg.InputModes:X}",
                        PortInformationForPossibleModeCombinationsMessage msg => $"Port Information (combinations) - Port {msg.PortId} Combinations: {string.Join(",", msg.ModeCombinations.Select(x => x.ToString("X")))}",
                        UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {BytesStringUtil.DataToString(data)}",
                        var unknown => $"{unknown.MessageType} (not yet formatted)",
                    };

                    logger.LogInformation(messageAsString);
                });

                // byte[] request = MessageEncoder.Encode(new HubPropertyMessage() { Property = HubProperty.BatteryVoltage, Operation = HubPropertyOperation.RequestUpdate });
                // await kernel.SendBytesAsync(request);

                // logger.LogInformation("Request PortInformation (ModeInfo)");
                // request = MessageEncoder.Encode(new PortInformationRequestMessage() { PortId = 0, InformationType = PortInformationType.ModeInfo });
                // await kernel.SendBytesAsync(request);

                // logger.LogInformation("Request PortInformation (ModeCombinations)");
                // request = MessageEncoder.Encode(new PortInformationRequestMessage() { PortId = 0, InformationType = PortInformationType.PossibleModeCombinations });
                // await kernel.SendBytesAsync(request);

                logger.LogInformation("Request PortModeInformation(s)");
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat }));
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 1, InformationType = PortModeInformationType.ValueFormat }));
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 2, InformationType = PortModeInformationType.ValueFormat }));
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 3, InformationType = PortModeInformationType.ValueFormat }));
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 4, InformationType = PortModeInformationType.ValueFormat }));
                await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 5, InformationType = PortModeInformationType.ValueFormat }));

                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Raw }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Pct }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.SI }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Symbol }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Mapping }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.InternalUse }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.MotorBias }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.CapabilityBits }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat }));

                Console.ReadLine();

                logger.LogInformation("Switch off device");
                await kernel.SendBytesAsync(MessageEncoder.Encode(new HubActionMessage() { Action = HubAction.SwitchOffHub }));

                Console.ReadLine();
            }
        }
    }
}
