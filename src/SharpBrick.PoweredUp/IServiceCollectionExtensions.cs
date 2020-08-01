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
                .AddSingleton<PoweredUpHost>()
                .AddScoped<BluetoothKernel>()
                .AddScoped<IHubFactory, HubFactory>()
                .AddScoped<IDeviceFactory, DeviceFactory>()
                .AddScoped<IPoweredUpProtocol, PoweredUpProtocol>()
                .AddTransient<LinearMidCalibration>();
    }
}