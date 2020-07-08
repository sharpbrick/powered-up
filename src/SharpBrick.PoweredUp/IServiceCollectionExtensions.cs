using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Devices;

namespace SharpBrick.PoweredUp
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPoweredUp(this IServiceCollection self)
            => self
                .AddSingleton<IDeviceFactory, DeviceFactory>();
    }
}