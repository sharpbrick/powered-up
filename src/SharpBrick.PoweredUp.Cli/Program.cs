using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
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

                deviceApp.Command("pretty-print", prettyPrintApp =>
                {
                    prettyPrintApp.HelpOption();
                    var traceOption = prettyPrintApp.Option("--trace", "Enable Tracing", CommandOptionType.SingleValue);
                    var systemTypeOption = prettyPrintApp.Option("--t", "System Type (parsable number)", CommandOptionType.SingleValue);
                    var hubOption = prettyPrintApp.Option("--h", "Hub Id (decimal number)", CommandOptionType.SingleValue);
                    var portOption = prettyPrintApp.Option("--p", "Port Id (decimal number)", CommandOptionType.SingleValue);
                    var ioTypeOption = prettyPrintApp.Option("--io", "IO Type (hex number, e.g. 003C)", CommandOptionType.SingleValue);
                    var hwVersionOption = prettyPrintApp.Option("--hw", "Hardware Version", CommandOptionType.SingleValue);
                    var swVersionOption = prettyPrintApp.Option("--sw", "Software Version", CommandOptionType.SingleValue);
                    var fileOption = prettyPrintApp.Option("--file", "File to read from (otherwise stdin", CommandOptionType.SingleValue);

                    prettyPrintApp.OnExecuteAsync(async cts =>
                    {
                        var enableTrace = bool.TryParse(traceOption.Value(), out var x0) ? x0 : false;
                        var systemType = byte.TryParse(systemTypeOption.Value(), out var x1) ? x1 : (byte)0;
                        var hubId = byte.TryParse(hubOption.Value(), out var x2) ? x2 : (byte)0;
                        var portId = byte.TryParse(portOption.Value(), out var x3) ? x3 : (byte)0;
                        var ioType = ushort.TryParse(ioTypeOption.Value(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var x4) ? x4 : (ushort)0;
                        var hwVersion = Version.TryParse(hwVersionOption.Value(), out var x5) ? x5 : new Version("0.0.0.0");
                        var swVersion = Version.TryParse(swVersionOption.Value(), out var x6) ? x6 : new Version("0.0.0.0");
                        var file = fileOption.Value();

                        var serviceProvider = CreateServiceProviderWithMock(enableTrace);

                        using (var scope = serviceProvider.CreateScope())
                        {
                            var scopedServiceProvider = scope.ServiceProvider;

                            await AddTraceWriterAsync(scopedServiceProvider, enableTrace);

                            var prettyPrintCommand = scopedServiceProvider.GetService<PrettyPrint>(); // ServiceLocator ok: transient factory

                            TextReader reader = Console.In;

                            if (!string.IsNullOrWhiteSpace(file))
                            {
                                reader = new StringReader(File.ReadAllText(file));
                            }

                            await prettyPrintCommand.ExecuteAsync(reader, systemType, ioType, hubId, portId, hwVersion, swVersion);
                        }
                    });
                });

            });

            await app.ExecuteAsync(args);
        }

        private static IServiceCollection CreateServiceProviderInternal(bool enableTrace)
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
                .AddPoweredUp()

                // Add CLI Commands
                .AddTransient<DumpStaticPortInfo>()
                .AddTransient<DevicesList>()
                .AddTransient<PrettyPrint>();
        private static IServiceProvider CreateServiceProviderWithMock(bool enableTrace)
            => CreateServiceProviderInternal(enableTrace)
                .AddMockBluetooth()
                .BuildServiceProvider();
        private static IServiceProvider CreateServiceProvider(bool enableTrace)
            => CreateServiceProviderInternal(enableTrace)
                .AddWinRTBluetooth()
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
