using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleRemoteControlRssi : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var twoPortHandset = Host.FindByType<TwoPortHandset>();

        var device = twoPortHandset.RemoteControlRssi;

        await device.SetupNotificationAsync(device.ModeIndexRssi, true, deltaInterval: 1);

        var disposable = device.RssiObservable.Subscribe(x => Log.LogWarning($"RSSI: {x}"));

        await twoPortHandset.RgbLight.SetRgbColorsAsync(0x00, 0xFF, 0x00);

        Log.LogInformation("Watching RSSI changes for the next 20 seconds");

        await Task.Delay(20_000);

        disposable.Dispose();

        await twoPortHandset.SwitchOffAsync();
    }
}
