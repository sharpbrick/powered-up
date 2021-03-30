using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleTwoPortHubMediumLinearMotor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var twoPortHub = Host.FindByType<TwoPortHub>();

            await twoPortHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                .AddHub<TwoPortHub>(hubBuilder => hubBuilder
                    .AddDevice<MediumLinearMotor>(twoPortHub.A)
                )
            );

            var motor = twoPortHub.A.GetDevice<MediumLinearMotor>();

            await motor.SetAccelerationTimeAsync(3000);
            await motor.SetDecelerationTimeAsync(1000);
            await motor.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

            await Task.Delay(10_000);

            await motor.StartSpeedForDegreesAsync(180, -10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

            await Task.Delay(10_000);

            await motor.StartSpeedAsync(100, 90, SpeedProfiles.None);
            await Task.Delay(2000);
            await motor.StartSpeedAsync(-100, 90, SpeedProfiles.None);
            await Task.Delay(2000);
            await motor.StartSpeedAsync(0, 90, SpeedProfiles.None);

            await Task.Delay(10_000);

            await twoPortHub.SwitchOffAsync();
        }
    }
}