using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMotorInputCombinedMode
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputCombinedMode>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.VerifyAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub.A)
                    )
                );

                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexSpeed, motor.ModeIndexPosition, motor.ModeIndexAbsolutePosition);

                await motor.SetupNotificationAsync(motor.ModeIndexSpeed, true);
                await motor.SetupNotificationAsync(motor.ModeIndexPosition, true);
                await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

                await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

                await motor.StartPowerAsync(80);

                var disposable1 = motor.SpeedObservable.Subscribe(x => logger.LogWarning($"Speed: {x.SI} / {x.Pct}"));

                var disposable2 = motor.PositionObservable.Subscribe(x => logger.LogWarning($"Position: {x.SI} / {x.Pct}"));

                var disposable3 = motor.AbsolutePositionObservable.Subscribe(x => logger.LogWarning($"Absolute Position: {x.SI} / {x.Pct}"));

                await Task.Delay(2000);

                logger.LogWarning($"Position: {motor.AbsolutePosition}");

                await Task.Delay(2000);

                await motor.StartPowerAsync(0);

                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}