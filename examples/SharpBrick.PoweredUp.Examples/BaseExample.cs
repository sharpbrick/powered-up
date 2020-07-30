using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.WinRT;

namespace Example
{
    public abstract class BaseExample
    {
        protected PoweredUpHost host;
        protected IServiceProvider serviceProvider;
        public Hub selectedHub;

        public abstract Task ExecuteAsync();

        public virtual void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddPoweredUp();
        }

        public async Task InitHostAndDiscoverAsync(bool enableTrace)
        {
            InitHost(enableTrace);
            await DiscoverAsync(enableTrace);
        }

        public virtual Task DiscoverAsync(bool enableTrace)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Main");

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

            selectedHub = result;

            return Task.CompletedTask;
        }

        public void InitHost(bool enableTrace)
        {
            var serviceCollection = new ServiceCollection()
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
                .AddSingleton<IPoweredUpBluetoothAdapter, WinRTPoweredUpBluetoothAdapter>()
                ;

            Configure(serviceCollection);

            serviceProvider = serviceCollection.BuildServiceProvider();

            host = serviceProvider.GetService<PoweredUpHost>();
        }
    }
}