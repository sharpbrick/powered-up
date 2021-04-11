using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp
{
    public static class ServiceCollectionExtensionsForLogging
    {
        public static IServiceCollection AddPoweredUpConsoleLogging(this IServiceCollection self, IConfiguration configuration)
            => self.AddLogging(builder =>
               {
                   builder.ClearProviders();

                   if (bool.TryParse(configuration["EnableTrace"], out var enableTrace) && enableTrace)
                   {
                       builder.AddConsole();

                       builder.AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug);
                       builder.AddFilter("SharpBrick.PoweredUp.BlueGigaBLE.BlueGigaBLEPoweredUpBluetoothAdapater", LogLevel.Debug);
                   }
               });
    }
}