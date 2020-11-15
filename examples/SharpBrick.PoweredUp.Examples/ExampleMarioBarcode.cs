using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMarioBarcode : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var mario = Host.FindByType<MarioHub>())
            {
                await mario.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<MarioHub>(hubBuilder => { })
                );

                // for a normal scenario, a combined mode would not be needed. The tag sensor is able to detect brick colors AND the barcode on its own. The RGB scanner is neither very accurate and is scaled strangely.
                var tagSensor = mario.TagSensor;
                await tagSensor.TryLockDeviceForCombinedModeNotificationSetupAsync(tagSensor.ModeIndexTag, tagSensor.ModeIndexRgb);
                await tagSensor.SetupNotificationAsync(tagSensor.ModeIndexTag, true);
                await tagSensor.SetupNotificationAsync(tagSensor.ModeIndexRgb, true);
                await tagSensor.UnlockFromCombinedModeNotificationSetupAsync(true);

                var d1 = tagSensor.BarcodeObservable.Subscribe(x => Log.LogWarning($"Barcode: {x}"));
                var d2 = tagSensor.ColorNoObservable.Subscribe(x => Log.LogWarning($"Color No: {x}"));
                var d3 = tagSensor.RgbColorObservable.Subscribe(x => Log.LogWarning($"Color Rgb: {x.red}/{x.green}/{x.blue}"));

                await Task.Delay(20_000);

                d1.Dispose();
                d2.Dispose();
                d3.Dispose();

                await mario.SwitchOffAsync();
            }
        }
    }
}