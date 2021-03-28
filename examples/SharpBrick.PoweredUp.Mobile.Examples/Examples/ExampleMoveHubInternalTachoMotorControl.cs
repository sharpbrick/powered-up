using System.Threading.Tasks;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Mobile.Examples.Examples;

namespace Example
{
    public class ExampleMoveHubInternalTachoMotorControl : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var moveHub = Host.FindByType<MoveHub>())
            {
                var internalMotor = moveHub.MotorAtAB;
                await internalMotor.StartSpeedAsync(50, 100);
                await internalMotor.StartSpeedForTimeAsync(2000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(3000);
                await internalMotor.StopByBrakeAsync();

                var leftMotor = moveHub.LeftMotorAtB;
                var rightMotor = moveHub.RightMotorAtA;
                await leftMotor.StartSpeedAsync(10, 100);
                await leftMotor.StartSpeedForTimeAsync(1000, 10, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);
                await rightMotor.StartSpeedAsync(90, 100);
                await rightMotor.StartSpeedForTimeAsync(1000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(2000);

                await leftMotor.StopByBrakeAsync();
                await rightMotor.StopByBrakeAsync();

                await moveHub.SwitchOffAsync();
            }
        }
    }
}