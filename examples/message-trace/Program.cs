using System;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.WinRT;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBrick.PoweredUp.Examples.MessageTrace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConsole()
                    .AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug)
                    .AddFilter("SharpBrick.PoweredUp.PoweredUpHost", LogLevel.Debug)
                    .AddFilter("SharpBrick.PoweredUp.Protocol.PoweredUpProtocol", LogLevel.Debug))
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Main");

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            var host = new PoweredUpHost(poweredUpBluetoothAdapter, serviceProvider);
            TraceMessages tracer = null;

            logger.LogInformation("Finding Service");
            var cts = new CancellationTokenSource();
            host.Discover(async hub =>
            {
                logger.LogInformation("Connecting to Hub");

                hub.ConfigureProtocolAsync = async (protocol) =>
                {
                    tracer = new TraceMessages(protocol, serviceProvider.GetService<ILoggerFactory>().CreateLogger<TraceMessages>());

                    await tracer.ExecuteAsync();
                };

                await hub.ConnectAsync();

                logger.LogInformation(hub.AdvertisingName);
                logger.LogInformation(hub.SystemType.ToString());

                cts.Cancel();
            }, cts.Token);

            logger.LogInformation("Press any key to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.GotoAbsolutePositionAsync(45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.GotoAbsolutePositionAsync(-45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await technicMediumHub.SwitchOffAsync();
            }

            return;

            ulong bluetoothAddress = 158897336311065;

            if (bluetoothAddress == 0)
                return;


            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            using (var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()))
            {
                var protocol = new PoweredUpProtocol(kernel, loggerFactory.CreateLogger<PoweredUpProtocol>());

                await kernel.ConnectAsync();

                await protocol.SetupUpstreamObservableAsync();

                // virtual port sample
                // await protocol.SendMessageAsync(new VirtualPortSetupForConnectedMessage() { SubCommand = VirtualPortSubCommand.Connected, PortAId = 0x01, PortBId = 0x02, });
                // await kernel.SendBytesAsync(BytesStringUtil.StringToData("09-00-81-10-11-07-64-64-00")); // 3.27.5

                // single motor sample
                // await protocol.SendMessageAsync(BytesStringUtil.StringToData("09-00-81-00-11-07-64-64-00")); // 3.27.5

                //await protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage() { PortId = 99, Mode = 0x00, DeltaInterval = 5, NotificationEnabled = true });

                //await SetupPortInCombinedMode(protocol);

                Console.ReadLine();

                //await protocol.SendMessageAsync(new HubActionMessage() { Action = HubAction.ResetBusyIndication, });

                var motor = new TechnicXLargeLinearMotor(protocol, 0, 0);
                // await motor.SetAccelerationTime(3000);
                // await motor.SetDeccelerationTime(1000);
                // await motor.StartSpeedForTimeAsync(6000, 90, 100, PortOutputCommandSpecialSpeed.Hold, PortOutputCommandSpeedProfile.AccelerationProfile | PortOutputCommandSpeedProfile.DeccelerationProfile);

                // await Task.Delay(2000);

                //await motor.StartSpeedForDegrees(180, -10, 100, PortOutputCommandSpecialSpeed.Brake, PortOutputCommandSpeedProfile.None);

                await Task.Delay(2000);

                await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

                await motor.StartPowerAsync(80);

                logger.LogWarning($"XXXXXXXXXXXXXXXXXXXXXXXXXXXXX: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                logger.LogWarning($"XXXXXXXXXXXXXXXXXXXXXXXXXXXXX: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                await motor.StartPowerAsync(0);

                logger.LogWarning($"XXXXXXXXXXXXXXXXXXXXXXXXXXXXX: {motor.AbsolutePosition}");
                // await motor.StartSpeedAsync(100, 90, PortOutputCommandSpeedProfile.None);

                // await Task.Delay(2000);

                // await motor.StartSpeedAsync(127, 90, PortOutputCommandSpeedProfile.None);

                // await motor.StartSpeedAsync(-100, 90, PortOutputCommandSpeedProfile.None);
                // await Task.Delay(2000);

                // await motor.StartSpeedAsync(0, 90, PortOutputCommandSpeedProfile.None);

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
