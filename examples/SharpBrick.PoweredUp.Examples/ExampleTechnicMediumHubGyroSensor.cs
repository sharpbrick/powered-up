using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleTechnicMediumHubGyroSensor : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var device = technicMediumHub.GyroSensor;

        await device.SetupNotificationAsync(device.ModeIndexRotation, true);

        var disposable = device.RotationObservable.Subscribe(x => Log.LogWarning($"Rotation: {x.x} / {x.y} / {x.z}"));

        await Task.Delay(10_000);

        disposable.Dispose();

        await technicMediumHub.SwitchOffAsync();
    }
}
