using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleTechnicMediumHubTiltSensor : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var device = technicMediumHub.TiltSensor;

        // await device.TiltConfigOrientationAsync(TiltConfigOrientation.Front); // does not work on technic medium hub

        await device.SetupNotificationAsync(device.ModeIndexPosition, true);

        using var disposable = device.PositionObservable.Subscribe(x => Log.LogWarning($"Tilt: {x.x} / {x.y} / {x.z}"));

        await Task.Delay(10_000);

        await technicMediumHub.SwitchOffAsync();
    }
}
