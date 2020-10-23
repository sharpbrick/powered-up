using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleRemoteControlButton : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var twoPortHandset = Host.FindByType<TwoPortHandset>())
            {
                var device = twoPortHandset.A;

                await device.SetupNotificationAsync(device.ModeIndexBitField, true, deltaInterval: 1);

                await twoPortHandset.RgbLight.SetRgbColorsAsync(0xFF, 0x00, 0x00);

                Log.LogInformation("Press the '+' button on port A (left)");

                await device.PlusObservable.Any(x => x);

                Log.LogInformation("Thanks! Now press the 'red' button on port A (left)");

                await device.RedObservable.Any(x => x);

                Log.LogInformation("Thanks! Now press the '-' button on port A (left)");

                await device.MinusObservable.Any(x => x);

                Log.LogInformation("Thanks! Now press the '+' and '-' buttons on port A (left) at the same time");

                await device.ButtonsObservable.Any(x => x.Minus && x.Plus);

                var disposable = device.ButtonsObservable.Subscribe(x => Log.LogWarning($"Buttons: {x.Plus} - {x.Stop} - {x.Minus}"));
                var disposable2 = device.PlusObservable.Subscribe(x => Log.LogWarning($"Plus: {x}"));
                var disposable3 = device.RedObservable.Subscribe(x => Log.LogWarning($"Red: {x}"));
                var disposable4 = device.MinusObservable.Subscribe(x => Log.LogWarning($"Minus: {x}"));

                Log.LogInformation("Thanks! You now have 20 seconds to press any button combinations");

                await Task.Delay(20_000);

                disposable.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
                disposable4.Dispose();

                await twoPortHandset.SwitchOffAsync();
            }
        }
    }
}