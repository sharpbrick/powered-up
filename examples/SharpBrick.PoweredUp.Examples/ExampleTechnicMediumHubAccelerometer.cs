using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumHubAccelerometer
    {
        public static async Task ExecuteAsync()
        {
            var (host, serviceProvider, _) = ExampleHubDiscover.CreateHostAndDiscover();

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device = technicMediumHub.Accelerometer;

                await device.SetupNotificationAsync(device.ModeIndexGravity, true);

                var disposable = device.GravityObservable.Subscribe(x => logger.LogWarning($"Gravity: {x.x} / {x.y} / {x.z}"));

                await Task.Delay(60_000);

                disposable.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}