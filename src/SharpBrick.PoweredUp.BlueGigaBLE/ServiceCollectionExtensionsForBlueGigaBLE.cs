using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SharpBrick.PoweredUp.BlueGigaBLE;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp
{
    public static class ServiceCollectionExtensionsForBlueGigaBLE
    {
        /// <summary>
        /// Adds BlueGigaBLEBluetooth to the service-collection using parameters to define the COM-Port and wether extended logging to the ILogger shall be used
        /// </summary>
        /// <param name="services">The service-collection the service shall be added to</param>
        /// <param name="setupAction">The BlueGigaBLEOptions-action taken during Configure</param>
        /// <returns></returns>
        public static IServiceCollection AddBlueGigaBLEBluetooth(this IServiceCollection services, Action<BlueGigaBLEOptions> setupAction)
        {
            _ = services.Configure(setupAction);
            _ = services.AddSingleton<IPoweredUpBluetoothAdapter, BlueGigaBLEPoweredUpBluetoothAdapater>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BlueGigaBLEOptions>>();
                return new BlueGigaBLEPoweredUpBluetoothAdapater(comPortName: options.Value.COMPortName, traceDebug: options.Value.TraceDebug, logger: provider.GetRequiredService<ILogger<BlueGigaBLEPoweredUpBluetoothAdapater>>());
            }
            );
            return new BlueGigaBluetoothBuilder(services).Services;
        }

        public static IBlueGigaBluetoothBuilder SetBlueGigaBLEOptions(this IBlueGigaBluetoothBuilder builder, BlueGigaBLEOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _ = builder.Services.Configure<BlueGigaBLEOptions>(o =>
            {
                o.COMPortName = options.COMPortName;
                o.TraceDebug = options.TraceDebug;
            });
            return builder;
        }

        public interface IBlueGigaBluetoothBuilder
        {
            IServiceCollection Services { get; }
        }

        public class BlueGigaBluetoothBuilder : IBlueGigaBluetoothBuilder
        {
            public IServiceCollection Services { get; }
            public BlueGigaBluetoothBuilder(IServiceCollection services)
            {
                Services = services;
            }
        }
    }
}
