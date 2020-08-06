using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMotorVirtualPort : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var technicMediumHub = Host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(0)
                        .AddDevice<TechnicXLargeLinearMotor>(2)
                    )
                );

                var virtualPort = await technicMediumHub.CreateVirtualPortAsync(0, 2);

                var motorsOnVirtualPort = virtualPort.GetDevice<TechnicXLargeLinearMotor>();

                await motorsOnVirtualPort.GotoAbsolutePositionAsync(90, -45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await Task.Delay(3000);

                await motorsOnVirtualPort.StartSpeedAsync(100, 50, 100, SpeedProfiles.None);

                await Task.Delay(2000);

                await motorsOnVirtualPort.StartPowerAsync(10, 100);

                await Task.Delay(3000);

                await motorsOnVirtualPort.StartPowerAsync(0, 0);

                await Task.Delay(1000);


                await technicMediumHub.CloseVirtualPortAsync(virtualPort.PortId);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}