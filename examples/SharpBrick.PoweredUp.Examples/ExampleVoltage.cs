using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleVoltage
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var device = technicMediumHub.Voltage;

                await device.SetupNotificationAsync(device.ModeIndexVoltageL, true);

                var disposable = device.VoltageLObservable.Subscribe(x => logger.LogWarning($"Voltage L: {x.SI}mV ({x.Pct}%)"));

                await Task.Delay(60_000);

                disposable.Dispose();

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}