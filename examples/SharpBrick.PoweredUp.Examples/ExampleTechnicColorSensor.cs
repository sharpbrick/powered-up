using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTechnicColorSensor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var hub = Host.FindByType<TechnicMediumHub>();

            await hub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                    .AddDevice<TechnicColorSensor>(hub.A)
                )
            );

            var technicColorSensor = hub.A.GetDevice<TechnicColorSensor>();

            var m1 = technicColorSensor.ColorObservable.Subscribe(x => Log.LogWarning($"Color: {x}"));
            var m2 = technicColorSensor.ReflectionObservable.Subscribe(x => Log.LogWarning($"Reflection: {x}"));
            var m3 = technicColorSensor.AmbientLightObservable.Subscribe(x => Log.LogWarning($"Ambient Light: {x}"));
            var m6 = technicColorSensor.RgbObservable.Subscribe(x => Log.LogWarning($"RGB: {x.red}/{x.green}/{x.blue}/{x.fourthValue}"));
            var m7 = technicColorSensor.HsvObservable.Subscribe(x => Log.LogWarning($"RGB: {x.hue}/{x.saturation}/{x.value}"));

            await technicColorSensor.SetupNotificationAsync(technicColorSensor.ModeIndexColor, true);

            await Task.Delay(20_000);

            // set only subset of the lights
            await technicColorSensor.SetSectorLightAsync(0, 0, 100);


            //await technicColorSensor.SetRgbColors2Async(100, 0, 0);

            await Task.Delay(5_000);

            await hub.SwitchOffAsync();
        }
    }
}