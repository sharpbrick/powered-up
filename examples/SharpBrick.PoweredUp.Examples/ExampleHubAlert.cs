using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public static class ExampleHubAlert
    {
        public static async Task ExecuteAsync(PoweredUpHost host, IServiceProvider serviceProvider, Hub selectedHub)
        {
            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                technicMediumHub.AlertObservable.Subscribe(x => Console.WriteLine($"Alert: {x}"));

                await technicMediumHub.EnableAlertNotificationAsync(HubAlert.LowSignalStrength);

                await Task.Delay(60_000);

                await technicMediumHub.DisableAlertNotification(HubAlert.LowSignalStrength);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}