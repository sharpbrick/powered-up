using System;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public static class ExampleMotorVirtualPort
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.CreateVirtualPortAsync(0, 2);
                await Task.Delay(1000);

                var virtualPort = technicMediumHub.Ports.FirstOrDefault(p => p.IsVirtual);
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