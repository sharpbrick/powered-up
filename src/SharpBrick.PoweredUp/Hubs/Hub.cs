using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub : IDisposable
    {
        private IDisposable _protocolListenerDisposable;
        private readonly ILogger _logger;

        public IPoweredUpProtocol Protocol { get; private set; }
        public byte HubId { get; }

        public Func<IPoweredUpProtocol, Task> ConfigureProtocolAsync { get; set; } = null;
        public IServiceProvider ServiceProvider { get; }

        public bool IsConnected => Protocol != null;

        public Hub(byte hubId, IServiceProvider serviceProvider, Port[] knownPorts)
        {
            HubId = hubId;
            ServiceProvider = serviceProvider;
            AddKnownPorts(knownPorts ?? throw new ArgumentNullException(nameof(knownPorts)));
            _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Hub>();
        }

        public void ConnectWithBluetoothAdapter(IPoweredUpBluetoothAdapter poweredUpBluetoothAdapter, ulong bluetoothAddress)
        {
            var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();

            _logger?.LogDebug("Init Hub with BluetoothKernel");
            var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>());
            _logger?.LogDebug("Init Hub with PoweredUpProtocol");
            Protocol = new PoweredUpProtocol(kernel, loggerFactory.CreateLogger<PoweredUpProtocol>());
        }

        #region Disposable Pattern
        ~Hub() => Dispose(false);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            _protocolListenerDisposable?.Dispose();
            Protocol?.Dispose();
        }

        #endregion

        // .. .INotifyPropertyChanged
        // hub alerts as event(s)

        public async Task ConnectAsync()
        {
            if (ConfigureProtocolAsync != null)
            {
                await ConfigureProtocolAsync(Protocol);
            }

            _protocolListenerDisposable = Protocol.UpstreamMessages
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

            _logger?.LogDebug("Connecting BluetoothKernel");
            await Protocol.ConnectAsync();

            _logger?.LogDebug("Query Hub Properties");
            await InitialHubPropertiesQueryAsync();

            //TODO await properties

            //TODO HubId = hubId;

            _logger?.LogDebug("Finished Querying Hub Properties");
        }

        private void OnHubChange(PoweredUpMessage message)
        {
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