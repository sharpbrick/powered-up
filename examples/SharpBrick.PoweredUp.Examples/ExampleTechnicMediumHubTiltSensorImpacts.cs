using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumHubTiltSensorImpacts : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device = technicMediumHub.TiltSensor;

                await device.TiltConfigImpactAsync(2, 1270);

                await device.SetupNotificationAsync(device.ModeIndexImpacts, true, deltaInterval: 1);

                using var disposable = device.ImpactsObservable.Subscribe(x => logger.LogWarning($"Impact: {x.SI} / {x.Pct} / {x.Raw}"));

                await Task.Delay(10_000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}