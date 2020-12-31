using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Hubs;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPoweredUp(this IServiceCollection self)
            => self
                // global infrastructure
                .AddSingleton<PoweredUpHost>()

                // per connection infrastructure
                .AddScoped<BluetoothKernel>()
                .AddScoped<IHubFactory, HubFactory>()
                .AddScoped<IDeviceFactory, DeviceFactory>()
                .AddScoped<ILegoWirelessProtocol, LegoWirelessProtocol>()

                // hubs
                .AddTransient<TwoPortHub>()
                .AddTransient<TwoPortHandset>()
                .AddTransient<TechnicMediumHub>()
                .AddTransient<MarioHub>()
                .AddTransient<DuploTrainBaseHub>()
                .AddTransient<MoveHub>()

                // functions
                .AddTransient<DiscoverPorts>()
                .AddTransient<TraceMessages>()
                .AddTransient<LinearMidCalibration>()
                .AddTransient<LinearSpeedChange>()
                ;
    }
}