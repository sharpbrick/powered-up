using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumHubTiltSensor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device = technicMediumHub.TiltSensor;

                // await device.TiltConfigOrientationAsync(TiltConfigOrientation.Front); // does not work on technic medium hub

                await device.SetupNotificationAsync(device.ModeIndexPosition, true);

                using var disposable = device.PositionObservable.Subscribe(x => logger.LogWarning($"Tilt: {x.x} / {x.y} / {x.z}"));

                await Task.Delay(10_000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}