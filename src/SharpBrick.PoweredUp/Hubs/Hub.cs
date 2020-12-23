using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub : IDisposable
    {
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ILogger _logger;
        private readonly IDeviceFactory _deviceFactory;
        private readonly SystemType _knownSystemType;

        public ILegoWirelessProtocol Protocol { get; private set; }
        public byte HubId { get; private set; }
        public IServiceProvider ServiceProvider { get; }
        public bool IsConnected => Protocol != null;

        public Hub(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<Hub> logger, IServiceProvider serviceProvider, SystemType knownSystemType, Port[] knownPorts, IEnumerable<HubProperty> knownProperties = default)
        {
            Protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(deviceFactory));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _knownSystemType = knownSystemType;
            AddKnownPorts(knownPorts ?? throw new ArgumentNullException(nameof(knownPorts)));
            _logger = logger;

            SetupHubProperties(knownProperties);
            SetupOnHubChange();
            SetupOnPortChangeObservable(Protocol.UpstreamMessages);
            SetupHubAlertObservable(Protocol.UpstreamMessages);
            SetupHubPropertyObservable(Protocol.UpstreamMessages);
        }

        public void Configure(byte hubId)
        {
            HubId = hubId;
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
            await Protocol.ConnectAsync(_knownSystemType);

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

        private void OnHubChange(LegoWirelessMessage message)
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