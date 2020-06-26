using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.WinRT;

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

                        var loggerFactory = CreateLoggerFactory(enableTrace);
                        var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

                        ulong bluetoothAddress = FindAndSelectHub(poweredUpBluetoothAdapter);

                        if (bluetoothAddress == 0)
                            return;

                        await DevicesList.ExecuteAsync(loggerFactory, poweredUpBluetoothAdapter, bluetoothAddress, enableTrace);
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

                        var loggerFactory = CreateLoggerFactory(enableTrace);
                        var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

                        ulong bluetoothAddress = FindAndSelectHub(poweredUpBluetoothAdapter);

                        if (bluetoothAddress == 0)
                            return;

                        var port = byte.Parse(portOption.Value());

                        await DumpStaticPortInfo.ExecuteAsync(loggerFactory, poweredUpBluetoothAdapter, bluetoothAddress, port, enableTrace);
                    });
                });
            });

            await app.ExecuteAsync(args);
        }

        private static ILoggerFactory CreateLoggerFactory(bool enableTrace)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();

                if (enableTrace)
                {
                    builder.AddConsole();

                    builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                }
            }
            );
        }

        private static ulong FindAndSelectHub(WinRTPoweredUpBluetoothAdapter poweredUpBluetoothAdapter)
        {
            ulong result = 0;
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

                result = selected.bluetoothAddresss; // default is 0

                if (result != default)
                {
                    Console.WriteLine($"Selected {selected.deviceType} with key {selected.key}");
                }
            }

            return result;
        }
    }
}
