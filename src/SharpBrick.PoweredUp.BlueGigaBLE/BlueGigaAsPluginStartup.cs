using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaAsPluginStartup : IPluginStartup
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddBlueGigaBLEBluetooth(options =>
                {
                    configuration.Bind(options);
                });
        }
    }
}