using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.WinRT;

namespace Example
{
    public static class ExampleHubDiscover
    {
        public static (PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub) CreateHostAndDiscover(bool enableTrace)
        {
            var serviceProvider = new ServiceCollection()
                // configure your favourite level of logging.
                .AddLogging(builder =>
                {
                    builder
                        .AddConsole();

                    if (enableTrace)
                    {
                        builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                    }
                })
                .AddPoweredUp()
                .BuildServiceProvider();


            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Main");

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            var host = new PoweredUpHost(poweredUpBluetoothAdapter, serviceProvider);

            Hub result = null;

            logger.LogInformation("Finding Service");
            var cts = new CancellationTokenSource();
            host.Discover(async hub =>
            {
                // add this when you are interested in a tracing of the message ("human readable")
                if (enableTrace)
                {
                    var tracer = new TraceMessages(hub.Protocol, serviceProvider.GetService<ILoggerFactory>().CreateLogger<TraceMessages>());

                    await tracer.ExecuteAsync();
                }

                logger.LogInformation("Connecting to Hub");
                await hub.ConnectAsync();

                result = hub;

                logger.LogInformation(hub.AdvertisingName);
                logger.LogInformation(hub.SystemType.ToString());

                cts.Cancel();

                logger.LogInformation("Press RETURN to continue to the action");
            }, cts.Token);

            logger.LogInformation("Press RETURN to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();
            return (host, serviceProvider, result);
        }
    }
}