using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace Example
{
    public class ExampleHubPropertyObserving : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleSetHubProperty>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                using var d1 = technicMediumHub.ButtonObservable.Subscribe(x => logger.LogInformation($"Buttom: {x}"));

                // optionally: trigger explicit request (like done during initialization)
                await technicMediumHub.RequestHubPropertySingleUpdate(HubProperty.Button);

                // and then observe it
                await technicMediumHub.SetupHubPropertyNotificationAsync(HubProperty.Button, true);

                await Task.Delay(5_000);

                await technicMediumHub.SetupHubPropertyNotificationAsync(HubProperty.Button, false);
                logger.LogInformation("Button no longer observed");

                await Task.Delay(5_000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}