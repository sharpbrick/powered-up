using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
            serviceCollection
                .AddPoweredUp();
        }

        public async Task InitExampleAndDiscoverAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            ServiceProvider = serviceProvider;

            Host = serviceProvider.GetService<PoweredUpHost>();

            Log = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Example");

            var enableTrace = bool.TryParse(configuration["EnableTrace"], out var x) && x;

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
            Console.ReadLine();

            cts.Cancel();

            SelectedHub = result;

            return Task.CompletedTask;
        }
    }
}