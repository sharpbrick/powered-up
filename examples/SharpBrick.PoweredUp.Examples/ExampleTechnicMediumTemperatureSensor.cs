using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumTemperatureSensor
    {
        public static async Task ExecuteAsync()
        {
            var (host, serviceProvider, _) = ExampleHubDiscover.CreateHostAndDiscover();

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device1 = technicMediumHub.Temperature1;

                await device1.SetupNotificationAsync(device1.ModeIndexTemperature, true);

                var disposable1 = device1.TemperatureObservable.Subscribe(x => logger.LogWarning($"Temperature 1: {x.SI}° ({x.Pct}%)"));

                var device2 = technicMediumHub.Temperature1;

                await device2.SetupNotificationAsync(device2.ModeIndexTemperature, true);

                var disposable2 = device2.TemperatureObservable.Subscribe(x => logger.LogWarning($"Temperature 2: {x.SI}° ({x.Pct}%)"));

                await Task.Delay(60_000);

                disposable1.Dispose();
                disposable2.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}