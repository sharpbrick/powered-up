using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleTechnicMediumHubAccelerometer : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var device = technicMediumHub.Accelerometer;

        await device.SetupNotificationAsync(device.ModeIndexGravity, true);

        var disposable = device.GravityObservable.Subscribe(x => Log.LogWarning($"Gravity: {x.x} / {x.y} / {x.z}"));

        await Task.Delay(10_000);

        disposable.Dispose();

        await technicMediumHub.SwitchOffAsync();
    }
}
