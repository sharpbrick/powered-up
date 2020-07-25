using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleSetHubProperty : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleSetHubProperty>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                using var d1 = technicMediumHub.AdvertisementNameObservable.Subscribe(x => logger.LogInformation($"From Observable for AdvertisementName: {x}"));
                using var d2 = technicMediumHub.PropertyChangedObservable.Subscribe(x => logger.LogInformation($"Property Changed: {x}"));

                var originalName = technicMediumHub.AdvertisingName;

                logger.LogInformation($"Original Name: {originalName}");

                await technicMediumHub.SetAdvertisingNameAsync("Hello World");

                logger.LogInformation($"New Name is: {technicMediumHub.AdvertisingName}");

                await technicMediumHub.ResetAdvertisingNameAsync();

                logger.LogInformation($"Resetted Name is: {technicMediumHub.AdvertisingName}");

                await technicMediumHub.SetAdvertisingNameAsync(originalName);

                logger.LogInformation($"Name Cleanup to: {originalName}");

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}