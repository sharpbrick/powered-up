using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicDistanceSensor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var hub = Host.FindByType<TechnicMediumHub>();

            await hub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                    .AddDevice<TechnicDistanceSensor>(hub.A))
            );

            var technicDistanceSensor = hub.A.GetDevice<TechnicDistanceSensor>();

            // measure distances (only single subscription)
            using var m1 = technicDistanceSensor.DistanceObservable.Subscribe(x => Log.LogWarning($"Distl: {x}"));
            using var m2 = technicDistanceSensor.ShortOnlyDistanceObservable.Subscribe(x => Log.LogWarning($"Dists: {x}"));
            using var m3 = technicDistanceSensor.SingleObservable.Subscribe(x => Log.LogWarning($"Singl: {x}"));

            await technicDistanceSensor.SetupNotificationAsync(technicDistanceSensor.ModeIndexDistance, true);

            // play with the eye lights (0-100)
            for (byte idx = 0; idx < 100; idx += 5)
            {
                await technicDistanceSensor.SetEyeLightAsync(0b0000_0000, idx, 0b0110_0100, idx);

                Log.LogWarning($"Brightness: {idx}");
                await Task.Delay(2_000);
            }

            await hub.SwitchOffAsync();
        }
    }
}