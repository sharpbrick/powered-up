using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public static class ExampleMotorControl
    {
        public static async Task ExecuteAsync()
        {
            var (host, serviceProvider, _) = ExampleHubDiscover.CreateHostAndDiscover();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.SetAccelerationTimeAsync(3000);
                await motor.SetDeccelerationTimeAsync(1000);
                await motor.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DeccelerationProfile);

                await Task.Delay(10_000);

                await motor.StartSpeedForDegreesAsync(180, -10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await Task.Delay(10_000);

                await motor.StartSpeedAsync(100, 90, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.StartSpeedAsync(127, 90, SpeedProfiles.None);
                await motor.StartSpeedAsync(-100, 90, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.StartSpeedAsync(0, 90, SpeedProfiles.None);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}