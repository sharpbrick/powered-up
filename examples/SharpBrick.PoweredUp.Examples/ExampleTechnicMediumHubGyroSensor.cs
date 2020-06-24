using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumHubGyroSensor
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device = technicMediumHub.GyroSensor;

                await device.SetupNotificationAsync(device.ModeIndexRotation, true);

                var disposable = device.RotationObservable.Subscribe(x => logger.LogWarning($"Rotation: {x.x} / {x.y} / {x.z}"));

                await Task.Delay(60_000);

                disposable.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}