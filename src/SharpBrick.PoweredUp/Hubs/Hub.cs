using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub : IDisposable
    {
        private IPoweredUpBluetoothAdapter _poweredUpBluetoothAdapter;
        private ulong _bluetoothAddress;
        private BluetoothKernel _kernel;
        private PoweredUpProtocol _protocol;
        private IDisposable _protocolListenerDisposable;

        private readonly ILogger _logger;

        public byte HubId { get; }

        public Hub(ILogger logger, Port[] knownPorts)
        {
            _logger = logger;
            _ports = knownPorts ?? throw new ArgumentNullException(nameof(knownPorts));
        }

        public void ConnectWithBluetoothAdapter(IPoweredUpBluetoothAdapter poweredUpBluetoothAdapter, ulong bluetoothAddress)
        {
            _poweredUpBluetoothAdapter = poweredUpBluetoothAdapter;
            _bluetoothAddress = bluetoothAddress;
        }

        #region Disposable Pattern
        ~Hub() => Dispose(false);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            _protocolListenerDisposable?.Dispose();
            _kernel?.Dispose();
        }

        #endregion

        // .. .INotifyPropertyChanged
        // hub alerts as event(s)

        public async Task ConnectAsync()
        {
            _logger?.LogDebug("Init Hub with BluetoothKernel");
            _kernel = new BluetoothKernel(_poweredUpBluetoothAdapter, _bluetoothAddress);
            _logger?.LogDebug("Init Hub with PoweredUpProtocol");
            _protocol = new PoweredUpProtocol(_kernel);

            _logger?.LogDebug("Connecting BluetoothKernel");
            await _kernel.ConnectAsync();
            await _protocol.SetupUpstreamObservableAsync();

            _protocolListenerDisposable = _protocol.UpstreamMessages
                .Where(msg => msg switch
                {
                    PortValueSingleMessage x => false,
                    PortValueCombinedModeMessage x => false,
                    PortInformationMessage x => false,
                    PortOutputCommandFeedbackMessage x => false,
                    PortInputFormatCombinedModeMessage x => false,
                    _ => true,
                })
                .Subscribe(OnHubChange);

            _logger?.LogDebug("Query Hub Properties");
            await InitialHubPropertiesQueryAsync();

            //TODO, query properties, await properties, setup monitor for ports

            //TODO HubId = hubId;

            _logger?.LogDebug("Finished Querying Hub Properties");
        }

        private void OnHubChange(PoweredUpMessage message)
        {
            _logger?.LogDebug("OnHubChange");

            if (message is HubPropertyMessage hubProperty && hubProperty.Operation == HubPropertyOperation.Update)
            {
                OnHubPropertyMessage(hubProperty);
            }
            else if (message is HubAttachedIOMessage hubAttachedIO)
            {
                OnHubAttachedIOMessage(hubAttachedIO);
            }
        }

        public async Task SwitchOffAsync()
        {
            var request = new HubActionMessage { Action = HubAction.SwitchOffHub };
            await _protocol.SendMessageAsync(request);

            //TODO await response
            //TODO HubAction.HubWillSwitchOff

            await _kernel.DisconnectAsync();
        }

        public async Task DisconnectAsync()
        {
            var request = new HubActionMessage { Action = HubAction.Disconnect };
            await _protocol.SendMessageAsync(request);

            //TODO await response
            //TODO HubAction.HubWillDisconnect

            await _kernel.DisconnectAsync();
        }
        public Task VccPortControlOn() => throw new NotImplementedException();
        public Task VccPortControlOff() => throw new NotImplementedException();
        public Task ActivateBusyIndicatorAsync() => throw new NotImplementedException();
        public Task ResetBusyIndicatorAsync() => throw new NotImplementedException();
        // TODO: Action HubWillGoIntoBootMode (eventy)
    }
}