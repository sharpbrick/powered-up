using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleColorDistanceSensor : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using (var twoPortHub = Host.FindByType<TwoPortHub>())
        {
            var colorDistSensor = twoPortHub.B.GetDevice<ColorDistanceSensor>();

            var observers = new[] {
                    colorDistSensor.ColorObservable
                        .Subscribe(color => Log.LogInformation("Color: {0}", color)),
                    colorDistSensor.RgbObservable
                        .Subscribe(rgb => Log.LogInformation("RGB: R: {0}, G: {1}, B: {2}", rgb.red, rgb.green, rgb.blue)),
                    colorDistSensor.ReflectionObservable
                        .Subscribe(refl => Log.LogInformation("Reflection: {0}", refl)),
                    colorDistSensor.AmbientLightObservable
                        .Subscribe(aml => Log.LogInformation("Ambient Light: {0}", aml)),
                    colorDistSensor.CountObservable
                        .Subscribe(cnt => Log.LogInformation("Count: {0}", cnt)),
                    colorDistSensor.ProximityObservable
                        .Subscribe(dst => Log.LogInformation("Proximity: {0}", dst)),
                };

            await TestMode(colorDistSensor, colorDistSensor.ModeIndexColor);
            await TestMode(colorDistSensor, colorDistSensor.ModeIndexRgb);
            await TestMode(colorDistSensor, colorDistSensor.ModeIndexReflection);
            await TestMode(colorDistSensor, colorDistSensor.ModeIndexAmbientLight);
            await TestMode(colorDistSensor, colorDistSensor.ModeIndexCount);
            await TestMode(colorDistSensor, colorDistSensor.ModeIndexProximity);

            foreach (var observer in observers)
            {
                observer.Dispose();
            }

            await twoPortHub.SwitchOffAsync();
        }

        async Task TestMode(ColorDistanceSensor colorDistSensor, byte mode)
        {
            Log.LogInformation("Teseting mode {0} - START", mode);
            await colorDistSensor.SetupNotificationAsync(mode, enabled: true);

            await Task.Delay(TimeSpan.FromMinutes(1));

            await colorDistSensor.SetupNotificationAsync(mode, enabled: false);
            Log.LogInformation("Teseting mode {0} - END", mode);
        }
    }
}
