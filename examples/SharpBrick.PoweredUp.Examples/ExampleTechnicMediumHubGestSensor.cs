using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicMediumHubGestSensor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

            var device = technicMediumHub.GestureSensor;

            await device.SetupNotificationAsync(0, true, deltaInterval: 0);

            using var _ = device.GestureObservable.Subscribe(x => Log.LogInformation($"Gesture {x}"));

            await Task.Delay(30_000);

            await technicMediumHub.SwitchOffAsync();
        }
    }
}