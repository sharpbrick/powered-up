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
                //await moveHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                //    .AddHub<MoveHub>(hubBuilder => hubBuilder
                //        .AddDevice<MoveHubInternalMotor>(moveHub.A)
                //    )
                //);

                var motorA = moveHub.A.GetDevice<MoveHubInternalMotor>();
                //var motorB = moveHub.B.GetDevice<MoveHubInternalMotor>();

                await motorA.StartSpeedAsync(50, 100);
                //await motorA.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                //await Task.Delay(10_000);

                //await motorB.SetAccelerationTimeAsync(3000);
                //await motorB.SetDecelerationTimeAsync(1000);
                //await motorB.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(10_000);

                //await moveHub.SwitchOffAsync();
            }
        }
    }
}