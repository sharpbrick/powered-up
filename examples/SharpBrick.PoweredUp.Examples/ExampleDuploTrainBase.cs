using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleDuploTrainBase : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var train = Host.FindByType<DuploTrainBaseHub>())
            {
                await train.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<DuploTrainBaseHub>(hubBuilder => { })
                );

                using var d1 = train.Voltage.VoltageSObservable.Subscribe(x => Log.LogWarning($"Voltage: {x.Pct}% / {x.SI}"));
                using var d2 = train.Motor.OnSecondsObservable.Subscribe(x => Log.LogWarning($"Seconds: {x}"));
                using var d3 = train.Speedometer.SpeedObservable.Subscribe(x => Log.LogWarning($"Speed: {x.SI}% / {x.SI}"));
                using var d4 = train.Speedometer.CountObservable.Subscribe(x => Log.LogWarning($"Count: {x}%"));
                using var d5 = train.ColorSensor.ColorObservable.Subscribe(x => Log.LogWarning($"Color: {x}"));
                using var d6 = train.ColorSensor.ColorTagObservable.Subscribe(x => Log.LogWarning($"Color Tag: {x}"));
                using var d7 = train.ColorSensor.ReflectionObservable.Subscribe(x => Log.LogWarning($"Reflection: {x}"));
                using var d8 = train.ColorSensor.RgbObservable.Subscribe(x => Log.LogWarning($"RGB {x.red}/{x.green}/{x.blue}"));

                await train.Voltage.SetupNotificationAsync(train.Voltage.ModeIndexVoltageS, true);

                // motor can either be queried for seconds active OR be instructed to run
                //await train.Motor.SetupNotificationAsync(train.Motor.ModeIndexOnSec, false);

                await train.Speedometer.TryLockDeviceForCombinedModeNotificationSetupAsync(train.Speedometer.ModeIndexSpeed, train.Speedometer.ModeIndexCount);
                await train.Speedometer.SetupNotificationAsync(train.Speedometer.ModeIndexSpeed, true, deltaInterval: 1);
                await train.Speedometer.SetupNotificationAsync(train.Speedometer.ModeIndexCount, true, deltaInterval: 1);
                await train.Speedometer.UnlockFromCombinedModeNotificationSetupAsync(true);

                await train.ColorSensor.TryLockDeviceForCombinedModeNotificationSetupAsync(train.ColorSensor.ModeIndexColor, train.ColorSensor.ModeIndexColorTag, train.ColorSensor.ModeIndexReflection, train.ColorSensor.ModeIndexRgb);
                await train.ColorSensor.SetupNotificationAsync(train.ColorSensor.ModeIndexColor, true, deltaInterval: 5);
                await train.ColorSensor.SetupNotificationAsync(train.ColorSensor.ModeIndexColorTag, true, deltaInterval: 5);
                await train.ColorSensor.SetupNotificationAsync(train.ColorSensor.ModeIndexReflection, true, deltaInterval: 5);
                await train.ColorSensor.SetupNotificationAsync(train.ColorSensor.ModeIndexRgb, true, deltaInterval: 5);
                await train.ColorSensor.UnlockFromCombinedModeNotificationSetupAsync(true);

                await train.Speaker.PlaySoundAsync(DuploTrainBaseSound.Horn);
                await train.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Red);
                await Task.Delay(1_000);
                await train.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Yellow);
                await Task.Delay(1_000);
                await train.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Green);
                await train.Speaker.PlaySoundAsync(DuploTrainBaseSound.StationDeparture);

                await train.Motor.StartPowerAsync(40);
                await Task.Delay(3_000);
                await train.Speaker.PlaySoundAsync(DuploTrainBaseSound.Steam);
                await train.Motor.StartPowerAsync(-40);
                await Task.Delay(3_000);
                await train.Speaker.PlaySoundAsync(DuploTrainBaseSound.Brake);
                await train.Motor.StopByFloatAsync();
                await Task.Delay(1_000);
                await train.Speaker.PlaySoundAsync(DuploTrainBaseSound.WaterRefill);

                await Task.Delay(1_000);

                await train.SwitchOffAsync();
            }
        }
    }
}