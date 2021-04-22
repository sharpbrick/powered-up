using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SharpBrick.PoweredUp.BlueGigaBLE;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp
{
    public static class ServiceCollectionExtensionsForBlueGigaBLE
    {
        /// <summary>
        /// Adds BlueGigaBLE Bluetooth Adapter. Requires a BlueGigaBLEOptions added to the Service Collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlueGigaBLEBluetooth(this IServiceCollection services)
            => services
                .AddSingleton<IPoweredUpBluetoothAdapter, BlueGigaBLEPoweredUpBluetoothAdapater>();

        /// <summary>
        /// Adds BlueGigaBLE Bluetooth Adapter and allow configuring it using a configuration callback
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlueGigaBLEBluetooth(this IServiceCollection services, Action<BlueGigaBLEOptions> setupAction)
            => services
                .Configure(setupAction)
                .AddBlueGigaBLEBluetooth();

        /// <summary>
        /// Adds BlueGigaBLE Bluetooth Adapter and allow configuring it using an IConfiguration object.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlueGigaBLEBluetooth(this IServiceCollection services, IConfiguration configuration)
            => services
                .Configure<BlueGigaBLEOptions>(configuration)
                .AddBlueGigaBLEBluetooth();
    }
}
