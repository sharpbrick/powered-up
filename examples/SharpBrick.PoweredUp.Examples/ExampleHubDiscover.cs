using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.WinRT;
using SharpBrick.PoweredUp.Functions;

namespace Example
{
    public static class ExampleHubDiscover
    {
        public static (PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub) CreateHostAndDiscover()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder
                    .AddConsole())
                .BuildServiceProvider();


            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Main");

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            var host = new PoweredUpHost(poweredUpBluetoothAdapter, serviceProvider);

            logger.LogInformation("Finding Service");
            var cts = new CancellationTokenSource();
            host.Discover(async hub =>
            {
                logger.LogInformation("Connecting to Hub");

                await hub.ConnectAsync();

                logger.LogInformation(hub.AdvertisingName);
                logger.LogInformation(hub.SystemType.ToString());

                cts.Cancel();

                logger.LogInformation("Press RETURN to continue to the action");
            }, cts.Token);

            logger.LogInformation("Press RETURN to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();
            return (host, serviceProvider, null);
        }
    }
}