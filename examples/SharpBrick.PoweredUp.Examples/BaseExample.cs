using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;

namespace Example
{
    public abstract class BaseExample
    {
        protected PoweredUpHost Host { get; set; }
        protected IServiceProvider ServiceProvider { get; set; }
        public Hub SelectedHub { get; set; }

        public ILogger Log { get; private set; }

        public abstract Task ExecuteAsync();

        public virtual void Configure(IServiceCollection serviceCollection)
        {
            _ = serviceCollection
                .AddPoweredUp();
        }

        public async Task InitHostAndDiscoverAsync(bool enableTrace, string bluetoothStackPort = "WINRT", bool enableTraceBlueGiga = false)
        {
            InitHost(enableTrace, bluetoothStackPort, enableTraceBlueGiga);

            Log = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("Example");

            await DiscoverAsync(enableTrace);
        }

        public virtual Task DiscoverAsync(bool enableTrace)
        {
            Hub result = null;

            Log.LogInformation("Finding Service");
            var cts = new CancellationTokenSource();
            Host.Discover(async hub =>
            {
                // add this when you are interested in a tracing of the message ("human readable")
                if (enableTrace)
                {
                    var tracer = hub.ServiceProvider.GetService<TraceMessages>();
                    await tracer.ExecuteAsync();
                }

                Log.LogInformation("Connecting to Hub");
                await hub.ConnectAsync();

                result = hub;

                Log.LogInformation(hub.AdvertisingName);
                Log.LogInformation(hub.SystemType.ToString());

                cts.Cancel();

                Log.LogInformation("Press RETURN to continue to the action");
            }, cts.Token);

            Log.LogInformation("Press RETURN to cancel Scanning");
            _ = Console.ReadLine();

            cts.Cancel();

            SelectedHub = result;

            return Task.CompletedTask;
        }

        public void InitHost(bool enableTrace, string bluetoothStackPort = "WINRT", bool enableTraceBlueGiga = false)
        {
            var serviceCollection = new ServiceCollection()
                // configure your favourite level of logging.
                .AddLogging(builder =>
                {
                    _ = builder
                        .AddConsole();

                    if (enableTrace)
                    {
                        _ = builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                    }
                    if (enableTraceBlueGiga)
                    {
                        _ = builder.AddFilter("SharpBrick.PoweredUp.BlueGigaBLE.BlueGigaBLEPoweredUpBluetoothAdapater", LogLevel.Debug);
                    }
                });
            if (bluetoothStackPort.Equals("WINRT", StringComparison.OrdinalIgnoreCase))
            {
                _ = serviceCollection.AddWinRTBluetooth();
            }
            else
            {
                //this adds the BlueGiga-implementation instead of the WinRT-implementation
                //the value of the parameter bluetoothStackPort is taken for the COM-Port on which the BlueGiga-adapter is connected
                _ = serviceCollection.AddBlueGigaBLEBluetooth(options =>
                  {
                      //enter the COMPort-Name here
                      //on Windows-PCs you can find it under Device Manager --> Ports (COM & LPT) --> Bleugiga Bluetooth Low Energy (COM#) (where # is a number)
                      options.COMPortName = bluetoothStackPort;
                      //setting this option to false supresses the complete LogDebug()-commands; so they will not generated at all
                      options.TraceDebug = enableTraceBlueGiga;
                  });
            }
            //can be easily extended here by taking another implementation (for example BlueZ for Raspberry) into the BluetoothImplementation-enum and then
            //do the needed Addxxx and options here:


            Configure(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            Host = ServiceProvider.GetService<PoweredUpHost>();
        }
    }
}