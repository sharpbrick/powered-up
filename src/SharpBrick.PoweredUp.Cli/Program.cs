using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Functions;

namespace SharpBrick.PoweredUp.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();
            app.UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue;

            app.Command("device", deviceApp =>
            {
                deviceApp.HelpOption();

                deviceApp.Command("list", deviceListApp =>
                {
                    deviceListApp.HelpOption();
                    var traceOption = deviceListApp.Option("--trace", "Enable Tracing", CommandOptionType.SingleValue);

                    deviceListApp.OnExecuteAsync(async cts =>
                    {
                        var enableTrace = bool.TryParse(traceOption.Value(), out var x) ? x : false;

                        var serviceProvider = CreateServiceProvider(enableTrace);
                        (ulong bluetoothAddress, SystemType systemType) = FindAndSelectHub(serviceProvider.GetService<IPoweredUpBluetoothAdapter>());

                        if (bluetoothAddress == 0)
                            return;

                        // initialize a DI scope per bluetooth connection / protocol (e.g. protocol is a per-bluetooth connection service)
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServiceProvider = scope.ServiceProvider;

                            await AddTraceWriterAsync(scopedServiceProvider, enableTrace);

                            scopedServiceProvider.GetService<BluetoothKernel>().BluetoothAddress = bluetoothAddress;

                            var deviceListCli = scopedServiceProvider.GetService<DevicesList>(); // ServiceLocator ok: transient factory

                            await deviceListCli.ExecuteAsync(systemType);
                        }
                    });
                });

                deviceApp.Command("dump-static-port", deviceDumpStaticPortApp =>
                {
                    deviceDumpStaticPortApp.HelpOption();
                    var traceOption = deviceDumpStaticPortApp.Option("--trace", "Enable Tracing", CommandOptionType.SingleValue);

                    var portOption = deviceDumpStaticPortApp.Option("-p", "Port to Dump", CommandOptionType.SingleValue);

                    deviceDumpStaticPortApp.OnExecuteAsync(async cts =>
                    {
                        var enableTrace = bool.TryParse(traceOption.Value(), out var x) ? x : false;

                        var serviceProvider = CreateServiceProvider(enableTrace);
                        (ulong bluetoothAddress, SystemType systemType) = FindAndSelectHub(serviceProvider.GetService<IPoweredUpBluetoothAdapter>());

                        if (bluetoothAddress == 0)
                            return;

                        // initialize a DI scope per bluetooth connection / protocol (e.g. protocol is a per-bluetooth connection service)
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServiceProvider = scope.ServiceProvider;

                            await AddTraceWriterAsync(scopedServiceProvider, enableTrace);

                            scopedServiceProvider.GetService<BluetoothKernel>().BluetoothAddress = bluetoothAddress;

                            var dumpStaticPortInfoCommand = scopedServiceProvider.GetService<DumpStaticPortInfo>(); // ServiceLocator ok: transient factory

                            var port = byte.Parse(portOption.Value());

                            await dumpStaticPortInfoCommand.ExecuteAsync(systemType, port);
                        }
                    });
                });
            });

            await app.ExecuteAsync(args);
        }

        private static IServiceProvider CreateServiceProvider(bool enableTrace)
            => new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();

                    if (enableTrace)
                    {
                        builder.AddConsole();

                        builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                    }
                })
                .AddWinRTBluetooth()
                .AddPoweredUp()

                // Add CLI Commands
                .AddTransient<DumpStaticPortInfo>()
                .AddTransient<DevicesList>()
                .BuildServiceProvider();

        public static async Task AddTraceWriterAsync(IServiceProvider serviceProvider, bool enableTrace)
        {
            if (enableTrace)
            {
                var traceMessages = serviceProvider.GetService<TraceMessages>(); // ServiceLocator ok: transient factory

                await traceMessages.ExecuteAsync();
            }
        }

        private static (ulong bluetoothAddress, SystemType systemType) FindAndSelectHub(IPoweredUpBluetoothAdapter poweredUpBluetoothAdapter)
        {
            ulong resultBluetooth = 0;
            SystemType resultSystemType = default;
            var devices = new ConcurrentBag<(int key, ulong bluetoothAddresss, PoweredUpHubManufacturerData deviceType)>();
            var cts = new CancellationTokenSource();
            int idx = 1;

            Console.WriteLine("Scan Started. Please select the Hub (using a number keys or 'q' to terminate):");

            poweredUpBluetoothAdapter.Discover(info =>
            {
                if (devices.FirstOrDefault(kv => kv.bluetoothAddresss == info.BluetoothAddress) == default)
                {
                    var deviceType = (PoweredUpHubManufacturerData)info.ManufacturerData[1];
                    devices.Add((idx, info.BluetoothAddress, deviceType));

                    Console.WriteLine($"{idx}: {(PoweredUpHubManufacturerData)info.ManufacturerData[1]} (with address {info.BluetoothAddress})");

                    idx++;
                }
                return Task.CompletedTask;
            }, cts.Token);

            var input = Console.ReadLine();

            cts.Cancel();

            if (int.TryParse(input, out var key))
            {
                var selected = devices.FirstOrDefault(kv => kv.key == key);

                resultBluetooth = selected.bluetoothAddresss; // default is 0
                resultSystemType = (SystemType)selected.deviceType;

                if (resultBluetooth != default)
                {
                    Console.WriteLine($"Selected {selected.deviceType} with key {selected.key}");
                }
            }

            return (resultBluetooth, resultSystemType);
        }
    }
}
