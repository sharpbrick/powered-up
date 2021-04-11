using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBrick.PoweredUp
{
    public interface IPluginStartup
    {
        void Configure(IServiceCollection services, IConfiguration configuration);
    }
}