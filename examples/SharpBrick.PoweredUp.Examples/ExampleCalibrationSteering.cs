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
            using (var technicMediumHub = Host.FindByType<TechnicMediumHub>())
            {
                var motor = technicMediumHub.A.GetDevice<TechnicLargeLinearMotor>();

                var calibration = ServiceProvider.GetService<LinearMidCalibration>();
                await calibration.ExecuteAsync(motor);
                await technicMediumHub.WaitButtonClickAsync();

                await motor.GotoPositionAsync(CW * 50, 20, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile);
                await technicMediumHub.WaitButtonClickAsync();

                await motor.GotoPositionAsync(CCW * 50, 20, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile);
                await Task.Delay(5000);

                await technicMediumHub.SwitchOffAsync();
            }
        }


    }
}