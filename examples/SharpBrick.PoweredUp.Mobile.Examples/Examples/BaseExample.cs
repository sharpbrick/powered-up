using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Functions;

namespace SharpBrick.PoweredUp.Mobile.Examples.Examples
{
    public abstract class BaseExample : IExample
    {
        private INativeDeviceInfo _nativeDeviceInfo;
        public BaseExample(INativeDeviceInfo nativeDeviceInfo)
        {
            _nativeDeviceInfo = nativeDeviceInfo;
        }

        protected PoweredUpHost Host { get; set; }

        protected ServiceProvider ServiceProvider { get; set; }

        public Hub SelectedHub { get; set; }

        public abstract Task ExecuteAsync();
        
        public virtual void Configure(IServiceCollection collection)
        {
            collection.AddPoweredUp();
        }

        public void InitHost(bool enableTrace)
        {
            var serviceCollection = new ServiceCollection()
                // configure your favourite level of logging.
               .AddLogging(builder =>
                {
                    builder.AddDebug();

                    if (enableTrace)
                    {
                        builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                    }
                })
               .AddXamarinBluetooth(_nativeDeviceInfo);

            Configure(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            Host = ServiceProvider.GetService<PoweredUpHost>();
        }

        public virtual async Task DiscoverAsync(bool enableTrace)
        {
            Hub result = null;

            // Log.LogInformation("Finding Service");
            var cts = new CancellationTokenSource();
            Host.Discover(async hub =>
            {
                // add this when you are interested in a tracing of the message ("human readable")
                if (enableTrace)
                {
                    var tracer = hub.ServiceProvider.GetService<TraceMessages>();

                    await tracer.ExecuteAsync();
                }

                // Log.LogInformation("Connecting to Hub");
                await hub.ConnectAsync();

                result = hub;

                // Log.LogInformation(hub.AdvertisingName);
                // Log.LogInformation(hub.SystemType.ToString());

                cts.Cancel();

                // Log.LogInformation("Press RETURN to continue to the action");
            }, cts.Token);

            await Task.Delay(15000, cts.Token);

            cts.Cancel();

            SelectedHub = result;
        }

        public async Task InitHostAndDiscoverAsync(bool enableTrace)
        {
            InitHost(enableTrace);

            // Log = ServiceProvider.GetService<ILoggerFactory>().CreateLogger("Example");

            await DiscoverAsync(enableTrace);
        }

    }
}
