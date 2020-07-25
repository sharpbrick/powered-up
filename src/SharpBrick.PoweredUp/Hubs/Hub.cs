using System;
using System.Reactive.Disposables;
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
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ILogger _logger;

        public IPoweredUpProtocol Protocol { get; private set; }
        public byte HubId { get; }
        public IServiceProvider ServiceProvider { get; }
        public bool IsConnected => Protocol != null;

        public Hub(byte hubId, IServiceProvider serviceProvider, Port[] knownPorts)
        {
            HubId = hubId;
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            AddKnownPorts(knownPorts ?? throw new ArgumentNullException(nameof(knownPorts)));
            _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Hub>();

        }

        public void ConnectWithBluetoothAdapter(IPoweredUpBluetoothAdapter poweredUpBluetoothAdapter, ulong bluetoothAddress)
        {
            _logger?.LogDebug("Init Hub with BluetoothKernel");
            var kernel = ActivatorUtilities.CreateInstance<BluetoothKernel>(ServiceProvider, poweredUpBluetoothAdapter, bluetoothAddress);
            _logger?.LogDebug("Init Hub with PoweredUpProtocol");
            Protocol = ActivatorUtilities.CreateInstance<PoweredUpProtocol>(ServiceProvider, kernel);

            SetupOnHubChange();
            SetupOnPortChangeObservable(Protocol.UpstreamMessages);
            SetupHubAlertObservable(Protocol.UpstreamMessages);
            SetupHubPropertyObservable(Protocol.UpstreamMessages);
        }

        #region Disposable Pattern
        ~Hub() => Dispose(false);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing)
        {
            _compositeDisposable?.Dispose();
            Protocol?.Dispose();
        }

        #endregion

        // .. .INotifyPropertyChanged
        // hub alerts as event(s)

        public async Task ConnectAsync()
        {
            var expectedDevicesCompletedTask = ExpectedDevicesCompletedAsync();

            _logger?.LogDebug("Connecting BluetoothKernel");
            await Protocol.ConnectAsync();

            await expectedDevicesCompletedTask;
            _logger?.LogDebug("Hub Attached IO expected devices completed");

            _logger?.LogDebug("Query Hub Properties");
            await InitialHubPropertiesQueryAsync();

            //TODO HubId = hubId;

            _logger?.LogDebug("Finished Querying Hub Properties");
        }

        private void SetupOnHubChange()
        {
            var disposable = Protocol.UpstreamMessages
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

            _compositeDisposable.Add(disposable);
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
    }
}