using System;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.WinRT;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Functions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

                technicMediumHub.Current.CurrentLObservable.Subscribe(v =>
                {
                    logger.LogWarning($"Current: {v.Pct}% {v.SI}mA / {technicMediumHub.Current.CurrentL}mA");
                });

                await technicMediumHub.Current.SetupNotificationAsync(0x00, true, 1);

                // simple motor control
                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.GotoAbsolutePositionAsync(45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.GotoAbsolutePositionAsync(-45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.StartPowerAsync(100);
                await Task.Delay(5000);
                await motor.StartPowerAsync(0);

                await technicMediumHub.SwitchOffAsync();
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
