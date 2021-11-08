using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleTechnicMediumAngularMotorGrey : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
            .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                .AddDevice<TechnicMediumAngularMotorGrey>(technicMediumHub.A)
            )
        );

        var motor = technicMediumHub.A.GetDevice<TechnicMediumAngularMotorGrey>();

        await motor.GotoPositionAsync(45, 50, 100);

        await Task.Delay(2000);

        await motor.SetZeroAsync();

        await Task.Delay(2000);

        await motor.StartSpeedForDegreesAsync(90, 50, 100);

        await Task.Delay(2000);

        // align physical and reset position to it.
        await motor.GotoPositionAsync(0, 50, 100);

        await Task.Delay(2000);

        // does not reset back to marked position on device
        await motor.GotoRealZeroAsync();

        await Task.Delay(2000);

        await technicMediumHub.SwitchOffAsync();
    }
}
