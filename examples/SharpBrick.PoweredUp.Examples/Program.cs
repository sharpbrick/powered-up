using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        // invoke (due to multi-targeting) with ...
        // dotnet run -f net5.0 --
        // dotnet run -f net5.0-windows10.0.19041.0 --

        // use command line parameters
        // --EnableTrace true
        // --BluetoothAdapter WinRT|BlueGigaBLE
        // --COMPortName COM4
        // -- TraceDebug true
        // or configure in poweredup.json
        static async Task Main(string[] args)
        {
            // (1) load a configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("poweredup.json", true)
                .AddCommandLine(args)
                .Build();

            // Example Selection: select the example to execute
            var exampleToExecute = configuration["Example"] ?? "Colors";

            var typeToExecute = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(x => x.FullName.StartsWith($"Example.Example{exampleToExecute}"));

            if (typeToExecute is null)
            {
                Console.WriteLine("Could not find example.");

                return;
            }

            var example = Activator.CreateInstance(typeToExecute) as Example.BaseExample;


            // (2) build the DI container
            var serviceCollection = new ServiceCollection();

            // (2a) configure your favourite level of logging.
            serviceCollection.AddLogging(builder =>
            {
                builder
                    .AddConsole();

                if (bool.TryParse(configuration["EnableTrace"], out var enableTrace) && enableTrace)
                {
                    builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                }
                if (bool.TryParse(configuration["TraceDebug"], out var traceDebug) && traceDebug)
                {
                    builder.AddFilter("SharpBrick.PoweredUp.BlueGigaBLE.BlueGigaBLEPoweredUpBluetoothAdapater", LogLevel.Debug);
                }
            });

            // (2b) add .AddPoweredUp() (we delegate this into the example because some examples need a different setup)
            example.Configure(serviceCollection);


            // (2c) add your favourite Bluetooth Adapter
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
                // on Windows-PCs you can find it under Device Manager --> Ports (COM & LPT) --> Bleugiga Bluetooth Low Energy (COM#) (where # is a number)
                // "COMPortName": "COM4",

                // setting this option to false supresses the complete LogDebug()-commands; so they will not generated at all
                // "TraceDebug": true
                serviceCollection.AddBlueGigaBLEBluetooth(configuration);
            }
#endif

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Examples Initialization
            await example.InitExampleAndDiscoverAsync(serviceProvider, configuration);

            // Example Execution
            if (example.SelectedHub is not null)
            {
                await example.ExecuteAsync();
            }
        }
    }
}
