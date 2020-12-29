using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMoveHubInternalTachoMotorControl : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var moveHub = Host.FindByType<MoveHub>())
            {
                var internalMotor = moveHub.InternalMotor;
                await internalMotor.StartSpeedAsync(50, 100);
                await internalMotor.StartSpeedForTimeAsync(2000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(2000);

                // This is if you have a linear motor plugged into port D (ie. R2D2)
                var externalMotor = moveHub.D.GetDevice<MediumLinearMotor>();

                await externalMotor.SetAccelerationTimeAsync(3000);
                await externalMotor.SetDecelerationTimeAsync(1000);
                await externalMotor.StartSpeedForTimeAsync(2000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(10000);

                await moveHub.SwitchOffAsync();
            }
        }
    }
}