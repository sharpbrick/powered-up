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
                    try
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
                            PortInputFormatSingleMessage msg => $"Port Input Format (Single) - Port {msg.PortId}, Mode {msg.Mode}, Threshold {msg.DeltaInterval}, Notification {msg.NotificationEnabled}",
                            PortInputFormatCombinedModeMessage msg => $"Port Input Format (Combined Mode) - Port {msg.PortId} UsedCombinationIndex {msg.UsedCombinationIndex} Enabled {msg.MultiUpdateEnabled} Configured Modes {string.Join(",", msg.ConfiguredModeDataSetIndex)}",
                            UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {BytesStringUtil.DataToString(data)}",
                            var unknown => $"{unknown.MessageType} (not yet formatted)",
                        };

                        logger.LogInformation(messageAsString);
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical(e, "Eception occurred in Handler");
                    }
                });

                // byte[] request = MessageEncoder.Encode(new HubPropertyMessage() { Property = HubProperty.BatteryVoltage, Operation = HubPropertyOperation.RequestUpdate });
                // await kernel.SendBytesAsync(request);

                // logger.LogInformation("Request PortInformation (ModeInfo)");
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInformationRequestMessage() { PortId = 99, InformationType = PortInformationType.ModeInfo }));

                // logger.LogInformation("Request PortInformation (ModeCombinations)");
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInformationRequestMessage() { PortId = 99, InformationType = PortInformationType.PossibleModeCombinations }));

                // logger.LogInformation("Request PortModeInformation(s)");
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 1, InformationType = PortModeInformationType.ValueFormat }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 2, InformationType = PortModeInformationType.ValueFormat }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 3, InformationType = PortModeInformationType.ValueFormat }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 4, InformationType = PortModeInformationType.ValueFormat }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 5, InformationType = PortModeInformationType.ValueFormat }));

                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Raw }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Pct }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.SI }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Symbol }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Mapping }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.InternalUse }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.MotorBias }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.CapabilityBits }));
                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat }));

                // await kernel.SendBytesAsync(BytesStringUtil.StringToData("09-00-81-00-11-07-64-64-00")); // 3.27.5

                // await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupSingleMessage() { PortId = 0, Mode = 0x03, DeltaInterval = 5, NotificationEnabled = true }));

                await SetupPortInCombinedMode(kernel);

                Console.ReadLine();

                logger.LogInformation("Switch off device");
                await kernel.SendBytesAsync(MessageEncoder.Encode(new HubActionMessage() { Action = HubAction.SwitchOffHub }));

                Console.ReadLine();
            }
        }

        private static async Task SetupPortInCombinedMode(BluetoothKernel kernel)
        {
            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupCombinedModeMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.LockDeviceForSetup,
            }));

            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupCombinedModeForSetModeDataSetMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination,
                CombinationIndex = 0, // should refer 0b0000_0000_0000_1110 => SPEED POS APOS
                ModeDataSets = new PortInputFormatSetupCombinedModeModeDataSet[] {
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x01, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x02, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x03, DataSet = 0, },
                    }
            }));

            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x01,
                DeltaInterval = 10,
                NotificationEnabled = true,
            }));

            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x02,
                DeltaInterval = 10,
                NotificationEnabled = true,
            }));


            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x03,
                DeltaInterval = 10,
                NotificationEnabled = true,
            }));


            await kernel.SendBytesAsync(MessageEncoder.Encode(new PortInputFormatSetupCombinedModeMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateEnabled,
            }));
        }
    }
}
