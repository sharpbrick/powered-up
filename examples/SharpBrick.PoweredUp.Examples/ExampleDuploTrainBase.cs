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

                await train.Voltage.SetupNotificationAsync(train.Voltage.ModeIndexVoltageS, true);

                // works exclusive vs. motor
                //await train.Motor.SetupNotificationAsync(train.Motor.ModeIndexOnSec, false);

                await train.Speedometer.TryLockDeviceForCombinedModeNotificationSetupAsync(train.Speedometer.ModeIndexSpeed, train.Speedometer.ModeIndexCount);
                await train.Speedometer.SetupNotificationAsync(train.Speedometer.ModeIndexSpeed, true, deltaInterval: 1);
                await train.Speedometer.SetupNotificationAsync(train.Speedometer.ModeIndexCount, true, deltaInterval: 1);
                await train.Speedometer.UnlockFromCombinedModeNotificationSetupAsync(true);

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