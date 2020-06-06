using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.WinRT;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Utils;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Devices;

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
                var protocol = new PoweredUpProtocol(kernel, loggerFactory.CreateLogger<PoweredUpProtocol>());

                await kernel.ConnectAsync();

                await protocol.ReceiveMessageAsync(message =>
                {
                    try
                    {
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
                            HubAttachedIOForAttachedVirtualDeviceMessage msg => $"Attached Virtual IO - Port {msg.PortId} with A {msg.PortAId} / B {msg.PortBId}  of type {msg.IOTypeId}",
                            HubAttachedIOForDetachedDeviceMessage msg => $"Dettached IO - Port {msg.PortId}",
                            GenericErrorMessage msg => $"Error - {msg.ErrorCode} from {(MessageType)msg.CommandType}",
                            PortInformationForModeInfoMessage msg => $"Port Information - Port {msg.PortId} Total Modes {msg.TotalModeCount} / Capabilities Output:{msg.OutputCapability}, Input:{msg.InputCapability}, LogicalCombinable:{msg.LogicalCombinableCapability}, LogicalSynchronizable:{msg.LogicalSynchronizableCapability} / InputModes: {msg.InputModes:X}, OutputModes: {msg.InputModes:X}",
                            PortInformationForPossibleModeCombinationsMessage msg => $"Port Information (Combinations) - Port {msg.PortId} Combinations: {string.Join(",", msg.ModeCombinations.Select(x => x.ToString("X")))}",
                            PortValueSingleMessage msg => "Port Values - " + string.Join(";", msg.Data.Select(d => d switch
                            {
                                PortValueData<sbyte> dd => $"Port {dd.PortId}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<short> dd => $"Port {dd.PortId}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<int> dd => $"Port {dd.PortId}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<float> dd => $"Port {dd.PortId}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                _ => "Undefined Data Type",
                            })),
                            PortValueCombinedModeMessage msg => $"Port Value (Combined Mode) - Port {msg.PortId} " + string.Join(";", msg.Data.Select(d => d switch
                            {
                                PortValueData<sbyte> dd => $"Mode {dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<short> dd => $"Mode {dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<int> dd => $"Mode {dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                PortValueData<float> dd => $"Mode {dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                                _ => "Undefined Data Type",
                            })),
                            PortInputFormatSingleMessage msg => $"Port Input Format (Single) - Port {msg.PortId}, Mode {msg.Mode}, Threshold {msg.DeltaInterval}, Notification {msg.NotificationEnabled}",
                            PortInputFormatCombinedModeMessage msg => $"Port Input Format (Combined Mode) - Port {msg.PortId} UsedCombinationIndex {msg.UsedCombinationIndex} Enabled {msg.MultiUpdateEnabled} Configured Modes {string.Join(",", msg.ConfiguredModeDataSetIndex)}",
                            UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {BytesStringUtil.DataToString(msg.Data)}",
                            var unknown => $"{unknown.MessageType} (not yet formatted)",
                        };

                        if (message is GenericErrorMessage)
                        {
                            logger.LogError(messageAsString);
                        }
                        else
                        {
                            logger.LogInformation(messageAsString);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical(e, "Eception occurred in Handler");
                    }

                    return Task.CompletedTask;
                });

                // await protocol.SendMessageAsync(new HubPropertyMessage() { Property = HubProperty.BatteryVoltage, Operation = HubPropertyOperation.RequestUpdate });

                // logger.LogInformation("Request PortInformation (ModeInfo)");
                // await protocol.SendMessageAsync(new PortInformationRequestMessage() { PortId = 99, InformationType = PortInformationType.ModeInfo });

                // logger.LogInformation("Request PortInformation (ModeCombinations)");
                // await protocol.SendMessageAsync(new PortInformationRequestMessage() { PortId = 99, InformationType = PortInformationType.PossibleModeCombinations });

                // logger.LogInformation("Request PortModeInformation(s)");
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 1, InformationType = PortModeInformationType.ValueFormat });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 2, InformationType = PortModeInformationType.ValueFormat });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 3, InformationType = PortModeInformationType.ValueFormat });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 4, InformationType = PortModeInformationType.ValueFormat });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 5, InformationType = PortModeInformationType.ValueFormat });

                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Raw });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Pct });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.SI });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Symbol });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.Mapping });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.InternalUse });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.MotorBias });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.CapabilityBits });
                // await protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = 0, Mode = 0, InformationType = PortModeInformationType.ValueFormat });

                // virtual port sample
                // await protocol.SendMessageAsync(new VirtualPortSetupForConnectedMessage() { SubCommand = VirtualPortSubCommand.Connected, PortAId = 0x01, PortBId = 0x02, });
                // await kernel.SendBytesAsync(BytesStringUtil.StringToData("09-00-81-10-11-07-64-64-00")); // 3.27.5

                // single motor sample
                // await protocol.SendMessageAsync(BytesStringUtil.StringToData("09-00-81-00-11-07-64-64-00")); // 3.27.5

                //await protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage() { PortId = 99, Mode = 0x00, DeltaInterval = 5, NotificationEnabled = true });

                //await SetupPortInCombinedMode(protocol);

                Console.ReadLine();

                //await protocol.SendMessageAsync(new HubActionMessage() { Action = HubAction.ResetBusyIndication, });

                var rgbLight = new RgbLight(protocol, 50);
                //await rgbLight.SetRgbColorNoAsync(PortOutputCommandColors.Pink);
                await rgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);
                //await rgbLight.SetRgbColorsAsync(0xFF, 0x00, 0x00);

                Console.ReadLine();

                logger.LogInformation("Switch off device");
                await protocol.SendMessageAsync(new HubActionMessage() { Action = HubAction.SwitchOffHub });

                Console.ReadLine();
            }
        }

        private static async Task SetupPortInCombinedMode(PoweredUpProtocol protocol)
        {
            await protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.LockDeviceForSetup,
            });

            await protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeForSetModeDataSetMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination,
                CombinationIndex = 0, // should refer 0b0000_0000_0000_1110 => SPEED POS APOS
                ModeDataSets = new PortInputFormatSetupCombinedModeModeDataSet[] {
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x01, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x02, DataSet = 0, },
                        new PortInputFormatSetupCombinedModeModeDataSet() { Mode = 0x03, DataSet = 0, },
                    }
            });

            await protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x01,
                DeltaInterval = 10,
                NotificationEnabled = true,
            });

            await protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x02,
                DeltaInterval = 10,
                NotificationEnabled = true,
            });


            await protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                PortId = 0,
                Mode = 0x03,
                DeltaInterval = 10,
                NotificationEnabled = true,
            });


            await protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeMessage()
            {
                PortId = 0,
                SubCommand = PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateEnabled,
            });
        }

    }
}
