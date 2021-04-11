using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBrick.PoweredUp.WinRT
{
    public class WinRTAsPluginStartup : IPluginStartup
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
            => services.AddWinRTBluetooth();
    }
}