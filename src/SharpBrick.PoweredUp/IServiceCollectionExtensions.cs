using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPoweredUp(this IServiceCollection self)
            => self
                .AddSingleton<IHubFactory, HubFactory>()
                .AddSingleton<IDeviceFactory, DeviceFactory>()
                .AddSingleton<PoweredUpHost>()
                .AddTransient<LinearMidCalibration>();
    }
}