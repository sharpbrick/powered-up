using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;
using static SharpBrick.PoweredUp.Directions;

namespace Example
{
    public class ExampleCalibrationSteering : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var motor = technicMediumHub.A.GetDevice<TechnicLargeLinearMotor>();

                var calibration = serviceProvider.GetService<LinearMidCalibration>();
                await calibration.ExecuteAsync(motor);

                await Task.Delay(5000);
                await motor.GotoAbsolutePositionAsync(CW * 50, 20, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile);
                await Task.Delay(5000);
                await motor.GotoAbsolutePositionAsync(CCW * 50, 20, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile);
                await Task.Delay(5000);

                await technicMediumHub.SwitchOffAsync();
            }
        }


    }
}