using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public static class ExampleMotorControl
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub.A)
                    )
                );

                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

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

                await motor.GotoAbsolutePositionAsync(45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.GotoAbsolutePositionAsync(-45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}