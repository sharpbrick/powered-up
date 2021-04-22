using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Functions;

namespace SharpBrick.PoweredUp.Cli
{
    class Program
    {
        public const int SuccessExitCode = 0;
        public const int UnknownOperationExitCode = 1;
        public const int BluetoothNoSelectedDeviceExitCode = 2;
        public const int ExceptionExitCode = 3;

        static async Task<int> Main(string[] args)
        {
            // load a configuration object
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("poweredup.json", true)
                .AddCommandLine(args)
                .Build();

            var enableTrace = bool.TryParse(configuration["EnableTrace"], out var enableTraceInConfig) && enableTraceInConfig;

            var app = new CommandLineApplication
            {
                Name = "poweredup",
                Description = "A command line interface to investigate LEGO Powered UP hubs and devices.",
                UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue,
            };

            app.HelpOption();
            app.OnExecute(() =>
            {
                Console.WriteLine($"See {app.Name} --help for Options");
                return Program.UnknownOperationExitCode;
            });

            app.Command("device", deviceApp =>
            {
                deviceApp.Description = "Options to enumerate devices on a hub";
                deviceApp.HelpOption();

                deviceApp.OnExecute(() =>
                {
                    Console.WriteLine($"See {app.Name} {deviceApp.Name} --help for Options");
                    return Program.UnknownOperationExitCode;
                });

                deviceApp.Command("list", deviceListApp =>
                {
                    deviceListApp.Description = "Inspect all devices declared on the Hub by using information gathered using the LEGO Wireless Protocol";
                    deviceListApp.HelpOption();

                    // pseudo option for configuration to create documentation
                    deviceListApp.Option("--EnableTrace", "Enable Tracing (default: no trace)", CommandOptionType.SingleValue);
                    deviceListApp.Option("--BluetoothAdapter", "Use a specified BLE adapter (e.g. WinRT or BlueGigaBLE). Defaults to WinRT. Some adapter might require additional parameters.", CommandOptionType.SingleValue);
                    deviceListApp.Option("--COMPortName", "Name of the COM port to connect the BluetoothAdapter (if applicable; e.g. BlueGigaBLE)", CommandOptionType.SingleValue);
                    deviceListApp.Option("--TraceDebug", "Create detailed tracing of the BluetoothAdapter (if applicable; e.g. BlueGigaBLE)", CommandOptionType.SingleValue);

                    deviceListApp.OnExecuteAsync(async cts =>
                    {
                        try
                        {
                            var serviceProvider = CreateServiceProvider(configuration);
                            (ulong bluetoothAddress, SystemType systemType) = FindAndSelectHub(serviceProvider.GetService<IPoweredUpBluetoothAdapter>());

                            if (bluetoothAddress == 0)
                                return Program.BluetoothNoSelectedDeviceExitCode;

                            // initialize a DI scope per bluetooth connection / protocol (e.g. protocol is a per-bluetooth connection service)
                            using (var scope = serviceProvider.CreateScope())
                            {
                                var scopedServiceProvider = scope.ServiceProvider;

                                await AddTraceWriterAsync(scopedServiceProvider, enableTrace);

                                scopedServiceProvider.GetService<BluetoothKernel>().BluetoothAddress = bluetoothAddress;

                                var deviceListCli = scopedServiceProvider.GetService<DevicesList>(); // ServiceLocator ok: transient factory

                                await deviceListCli.ExecuteAsync(systemType);
                            }

                            return Program.SuccessExitCode;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            return Program.ExceptionExitCode;
                        }
                    });
                });

                deviceApp.Command("dump-static-port", deviceDumpStaticPortApp =>
                {
                    deviceDumpStaticPortApp.Description = "Inspect a specific device on a Hub Port by using (non-dynamic) information gathered using the LEGO Wireless Protocol. Emits a binary dump (use list for human readable output).";
                    deviceDumpStaticPortApp.HelpOption();

                    // pseudo option for configuration to create documentation
                    deviceDumpStaticPortApp.Option("--EnableTrace", "Enable Tracing (default: no trace)", CommandOptionType.SingleValue);
                    deviceDumpStaticPortApp.Option("--BluetoothAdapter", "Use a specified BLE adapter (e.g. WinRT or BlueGigaBLE). Defaults to WinRT. Some adapter might require additional parameters.", CommandOptionType.SingleValue);
                    deviceDumpStaticPortApp.Option("--COMPortName", "Name of the COM port to connect the BluetoothAdapter (if applicable; e.g. BlueGigaBLE)", CommandOptionType.SingleValue);
                    deviceDumpStaticPortApp.Option("--TraceDebug", "Create detailed tracing of the BluetoothAdapter (if applicable; e.g. BlueGigaBLE)", CommandOptionType.SingleValue);

                    var portOption = deviceDumpStaticPortApp.Option("-p", "Port to Dump", CommandOptionType.SingleValue);
                    var headerOption = deviceDumpStaticPortApp.Option("-f", "Add Hub and IOType Header", CommandOptionType.NoValue);

                    deviceDumpStaticPortApp.OnExecuteAsync(async cts =>
                    {
                        try
                        {
                            var headerEnabled = headerOption.Values.Count > 0;

                            var serviceProvider = CreateServiceProvider(configuration);
                            (ulong bluetoothAddress, SystemType systemType) = FindAndSelectHub(serviceProvider.GetService<IPoweredUpBluetoothAdapter>());

                            if (bluetoothAddress == 0)
                                return Program.BluetoothNoSelectedDeviceExitCode;

                            // initialize a DI scope per bluetooth connection / protocol (e.g. protocol is a per-bluetooth connection service)
                            using (var scope = serviceProvider.CreateScope())
                            {
                                var scopedServiceProvider = scope.ServiceProvider;

                                await AddTraceWriterAsync(scopedServiceProvider, enableTrace);

                                scopedServiceProvider.GetService<BluetoothKernel>().BluetoothAddress = bluetoothAddress;

                                var dumpStaticPortInfoCommand = scopedServiceProvider.GetService<DumpStaticPortInfo>(); // ServiceLocator ok: transient factory

                                var port = byte.Parse(portOption.Value());

                                await dumpStaticPortInfoCommand.ExecuteAsync(systemType, port, headerEnabled);
                            }

                            return Program.SuccessExitCode;

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            return Program.ExceptionExitCode;
                        }
                    });
                });

                deviceApp.Command("pretty-print", prettyPrintApp =>
                {
                    prettyPrintApp.Description = "Pretty prints a previously recorded binary dump collected using dump-static-ports";
                    prettyPrintApp.HelpOption();

                    prettyPrintApp.Option("--EnableTrace", "Enable Tracing (default: no trace)", CommandOptionType.SingleValue);

                    var systemTypeOption = prettyPrintApp.Option("--t", "System Type (parsable number)", CommandOptionType.SingleValue);
                    var hubOption = prettyPrintApp.Option("--h", "Hub Id (decimal number)", CommandOptionType.SingleValue);
                    var portOption = prettyPrintApp.Option("--p", "Port Id (decimal number)", CommandOptionType.SingleValue);
                    var ioTypeOption = prettyPrintApp.Option("--io", "IO Type (hex number, e.g. 003C)", CommandOptionType.SingleValue);
                    var hwVersionOption = prettyPrintApp.Option("--hw", "Hardware Version", CommandOptionType.SingleValue);
                    var swVersionOption = prettyPrintApp.Option("--sw", "Software Version", CommandOptionType.SingleValue);
                    var fileOption = prettyPrintApp.Option("--file", "File to read from (otherwise stdin", CommandOptionType.SingleValue);

                    prettyPrintApp.OnExecuteAsync(async cts =>
                    {
                        try
                        {
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

                            return Program.SuccessExitCode;

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            return Program.ExceptionExitCode;
                        }
                    });
                });

            });

            return await app.ExecuteAsync(args);
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
                        builder.AddFilter("SharpBrick.PoweredUp.BlueGigaBLE.BlueGigaBLEPoweredUpBluetoothAdapater", LogLevel.Debug);
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
        private static IServiceProvider CreateServiceProvider(IConfiguration configuration)
        {
            var enableTrace = bool.TryParse(configuration["EnableTrace"], out var v) && v;

            var serviceCollection = CreateServiceProviderInternal(enableTrace);

            var bluetoothAdapter = configuration["BluetoothAdapter"] ?? "WinRT";
#if WINDOWS
            if (bluetoothAdapter == "WinRT")
            {
                serviceCollection.AddWinRTBluetooth();
            }
#endif

#if NET5_0_OR_GREATER
            if (bluetoothAdapter == "BlueGigaBLE")
            {
                // config for "COMPortName" and "TraceDebug" (either via command line or poweredup.json)
                serviceCollection.AddBlueGigaBLEBluetooth(options =>
                {
                    // on Windows-PCs you can find it under Device Manager --> Ports (COM & LPT) --> Bleugiga Bluetooth Low Energy (COM#) (where # is a number)
                    // "COMPortName": "COM4",

                    // setting this option to false supresses the complete LogDebug()-commands; so they will not generated at all
                    // "TraceDebug": true
                    configuration.Bind(options);
                });
            }
#endif
            return serviceCollection.BuildServiceProvider();
        }

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
