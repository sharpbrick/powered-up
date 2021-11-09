using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleTechnicMediumTemperatureSensor : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var device1 = technicMediumHub.Temperature1;

        await device1.SetupNotificationAsync(device1.ModeIndexTemperature, true);

        var disposable1 = device1.TemperatureObservable.Subscribe(x => Log.LogWarning($"Temperature 1: {x.SI}° ({x.Pct}%)"));

        var device2 = technicMediumHub.Temperature1;

        await device2.SetupNotificationAsync(device2.ModeIndexTemperature, true);

        var disposable2 = device2.TemperatureObservable.Subscribe(x => Log.LogWarning($"Temperature 2: {x.SI}° ({x.Pct}%)"));

        await Task.Delay(10_000);

        disposable1.Dispose();
        disposable2.Dispose();

        await technicMediumHub.SwitchOffAsync();
    }
}
