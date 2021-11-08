using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleMotorInputAbsolutePosition : BaseExample
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

        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Red);

        // align physical and reset position to it.
        await motor.GotoRealZeroAsync();

        await Task.Delay(2000);

        await motor.SetZeroAsync();

        await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexAbsolutePosition, motor.ModeIndexPosition);
        await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);
        await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
        await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

        using var disposable = motor.AbsolutePositionObservable.Subscribe(x => Log.LogWarning($"Absolute Position: {x.SI} / {x.Pct}"));
        using var disposable2 = motor.PositionObservable.Subscribe(x => Log.LogWarning($"Position: {x.SI} / {x.Pct}"));
        motor.PropertyChanged += (sender, ea) => { Log.LogInformation($"Change on Property {ea.PropertyName}"); };

        // relative movement from current positions
        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Pink);

        await motor.StartSpeedForDegreesAsync(30, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);
        await motor.StartSpeedForDegreesAsync(30, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);
        await motor.StartSpeedForDegreesAsync(30, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);

        // absolute movement to relative to zero point (SetZeroAsync or initial boot position)
        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Orange);

        await motor.GotoPositionAsync(0, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);
        await motor.GotoPositionAsync(90, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);
        await motor.GotoPositionAsync(180, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);
        await motor.GotoPositionAsync(-90, 20, 100, SpecialSpeed.Brake, SpeedProfiles.None);
        await Task.Delay(1000);

        await motor.GotoRealZeroAsync();
        await Task.Delay(2000);

        // time to turn the axis by hand to see notification feedback. Reformat input as GotoRealZero destroys them.
        await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexAbsolutePosition, motor.ModeIndexPosition);
        await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);
        await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
        await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Green);
        await motor.StopByFloatAsync();
        await Task.Delay(10_000);

        await technicMediumHub.SwitchOffAsync();
    }
}
