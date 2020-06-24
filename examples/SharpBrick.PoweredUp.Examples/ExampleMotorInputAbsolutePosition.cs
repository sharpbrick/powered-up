using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMotorInputAbsolutePosition
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

                await motor.StartPowerAsync(80);

                var disposable = motor.AbsolutePositionObservable.Subscribe(x => logger.LogWarning($"Position: {x.SI} / {x.Pct}"));

                await Task.Delay(2000);

                logger.LogWarning($"Position: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                await motor.StartPowerAsync(0);

                disposable.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}