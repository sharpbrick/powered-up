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
                .AddScoped<IPoweredUpProtocol, PoweredUpProtocol>()

                // hubs
                .AddTransient<TechnicMediumHub>()

                // functions
                .AddTransient<LinearMidCalibration>();
    }
}