using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleVoltage : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var device = technicMediumHub.Voltage;

        await device.SetupNotificationAsync(device.ModeIndexVoltageL, true);

        var disposable = device.VoltageLObservable.Subscribe(x => Log.LogWarning($"Voltage L: {x.SI}mV ({x.Pct}%)"));

        await Task.Delay(10_000);

        disposable.Dispose();

        await technicMediumHub.SwitchOffAsync();
    }
}
