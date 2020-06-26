using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        public async Task SwitchOffAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.SwitchOffHub
            });

            //TODO await response
            //TODO HubAction.HubWillSwitchOff

            await Protocol.DisconnectAsync();
        }

        public async Task DisconnectAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.Disconnect
            });

            //TODO await response
            //TODO HubAction.HubWillDisconnect

            await Protocol.DisconnectAsync();
        }
        public async Task VccPortControlOnAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.VccPortControlOn,
            });
        }


        public async Task VccPortControlOffAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.VccPortControlOff,
            });
        }

        public async Task ActivateBusyIndicatorAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.ActivateBusyIndication,
            });
        }
        public async Task ResetBusyIndicatorAsync()
        {
            AssertIsConnected();

            await Protocol.SendMessageAsync(new HubActionMessage
            {
                HubId = HubId,
                Action = HubAction.ResetBusyIndication,
            });
        }

        protected void AssertIsConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("The device needs to be connected to a protocol.");
            }
        }
    }
}