using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMotorInputCombinedMode : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

            await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                    .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub.A)
                )
            );

            var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

            await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexSpeed, motor.ModeIndexPosition, motor.ModeIndexAbsolutePosition);

            await motor.SetupNotificationAsync(motor.ModeIndexSpeed, true);
            await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
            await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

            await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

            await motor.StartPowerAsync(80);

            var disposable1 = motor.SpeedObservable.Subscribe(x => Log.LogWarning($"Speed: {x.SI} / {x.Pct}"));

            var disposable2 = motor.PositionObservable.Subscribe(x => Log.LogWarning($"Position: {x.SI} / {x.Pct}"));

            var disposable3 = motor.AbsolutePositionObservable.Subscribe(x => Log.LogWarning($"Absolute Position: {x.SI} / {x.Pct}"));

            await Task.Delay(2000);

            Log.LogWarning($"Position: {motor.AbsolutePosition}");

            await Task.Delay(2000);

            await motor.StartPowerAsync(0);

            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();

            await technicMediumHub.SwitchOffAsync();
        }
    }
}