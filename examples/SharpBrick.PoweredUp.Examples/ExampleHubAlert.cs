using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleHubAlert : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                technicMediumHub.AlertObservable.Subscribe(x => Console.WriteLine($"Alert: {x}"));

                await technicMediumHub.EnableAlertNotificationAsync(HubAlert.LowSignalStrength);

                await Task.Delay(60_000); // run

                await technicMediumHub.DisableAlertNotification(HubAlert.LowSignalStrength);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}