using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using Microsoft.Extensions.DependencyInjection;

namespace Example
{
    public class ExampleMotorInputAbsolutePosition
    {
        public static async Task ExecuteAsync()
        {
            var (host, serviceProvider, _) = ExampleHubDiscover.CreateHostAndDiscover();

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

                await motor.StartPowerAsync(80);

                logger.LogWarning($"Position: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                logger.LogWarning($"Position: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                await motor.StartPowerAsync(0);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}