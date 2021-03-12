using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
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
        /// Use parameters to define the COM-Port and wether extended logging to the ILogger shall be used
        /// </summary>
        /// <param name="services">The service-collection the service shall be added to</param>
        /// <returns></returns>
        //public static IServiceCollection AddBlueGigaBLEBluetooth(
        //    this IServiceCollection services,
        //    BlueGigaBLEOptions bleoptions)
        //{
        //    ILogger logger = services.Ge
        //    services.AddSingleton<IPoweredUpBluetoothAdapter, BlueGigaBLEPoweredUpBluetoothAdapater>(
        //    (adapter) => new BlueGigaBLEPoweredUpBluetoothAdapater(comPortName:bleoptions.COMPortName, traceDebug:bleoptions.TraceDebug ));
        //    return services;
        //}


        public static IServiceCollection AddBlueGigaBLEBluetooth(this IServiceCollection services, Action<BlueGigaBLEOptions> setupAction)
        {
            services.Configure(setupAction);
            services.AddSingleton<IPoweredUpBluetoothAdapter, BlueGigaBLEPoweredUpBluetoothAdapater>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BlueGigaBLEOptions>>();
                //services.PostConfigure<BlueGigaBLEPoweredUpBluetoothAdapater>(adapter =>
                //    {
                //        ILogger mylogger = services.BuildServiceProvider().GetRequiredService<ILogger>();
                //        adapter._logger = mylogger;
                //    }
                //);
                return new BlueGigaBLEPoweredUpBluetoothAdapater(comPortName: options.Value.COMPortName, traceDebug: options.Value.TraceDebug, logger: provider.GetRequiredService<ILogger<BlueGigaBLEPoweredUpBluetoothAdapater>>() );
            }
            );
            return new BlueGigaBluetoothBuilder(services).Services;
        }

        public static IBlueGigaBluetoothBuilder SetBlueGigaBLEOptions(this IBlueGigaBluetoothBuilder builder, BlueGigaBLEOptions options)
        {
            if (builder == null) throw new ArgumentException(nameof(builder));
            builder.Services.Configure<BlueGigaBLEOptions>(o =>
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
                this.Services = services;
            }
        }

        //((ctx) => {
        //    ILogger logger = ctx.GetRequiredService<ILogger>();
        //    return new BlueGigaBLEPoweredUpBluetoothAdapater(logger);
        //});
        //TODO: Make COM-Port and  other values for serial communication (baudrae) configurabel
        //TODO: .NET 5 Adding Action<TOptions> way for configuring COM-Port or alike
        //see https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors#actiontoptions-parameter

        //TODO: .NET 5 Options instance parameter
        //https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors#options-instance-parameter

        //TODO: .NET 5 IConfiguration parameter
        //https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors#iconfiguration-parameter
    }
}
