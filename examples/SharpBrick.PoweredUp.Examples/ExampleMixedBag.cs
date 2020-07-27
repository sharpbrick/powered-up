using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;
using static SharpBrick.PoweredUp.Directions;

namespace Example
{
    public class ExampleMixedBag : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub.A)
                    )
                );

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                technicMediumHub.Current.CurrentLObservable.Subscribe(v =>
                {
                    logger.LogWarning($"Current: {v.Pct}% {v.SI}mA / {technicMediumHub.Current.CurrentL}mA");
                });

                await technicMediumHub.Current.SetupNotificationAsync(0x00, true, 1);

                // simple motor control
                var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor.GotoAbsolutePositionAsync(CW * 45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.GotoAbsolutePositionAsync(CCW * 45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await Task.Delay(2000);
                await motor.StartPowerAsync(100);
                await Task.Delay(5000);
                await motor.StartPowerAsync(0);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}