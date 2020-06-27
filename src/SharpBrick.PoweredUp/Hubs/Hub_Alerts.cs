using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        public IObservable<HubAlert> AlertObservable { get; private set; }

        private void SetupHubAlertObservable(IObservable<PoweredUpMessage> upstreamMessages)
        {
            AlertObservable = upstreamMessages
                .OfType<HubAlertMessage>()
                .Where(msg => msg.DownstreamPayload == 0xFF)
                .Select(msg => msg.Alert);
        }

        public async Task EnableAlertNotificationAsync(HubAlert alert)
        {
            await Protocol.SendMessageAsync(new HubAlertMessage()
            {
                HubId = HubId,
                Operation = HubAlertOperation.EnableUpdates,
                Alert = alert,
            });
        }

        public async Task DisableAlertNotification(HubAlert alert)
        {
            await Protocol.SendMessageAsync(new HubAlertMessage()
            {
                HubId = HubId,
                Operation = HubAlertOperation.DisableUpdates,
                Alert = alert,
            });
        }
    }
}