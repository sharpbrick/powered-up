using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public class LegoWirelessProtocol : ILegoWirelessProtocol
    {
        private readonly BluetoothKernel _kernel;
        private readonly ILogger<LegoWirelessProtocol> _logger;
        private readonly IDeviceFactory _deviceFactory;
        private Subject<(byte[] data, LegoWirelessMessage message)> _upstreamSubject = null;

        public ProtocolKnowledge Knowledge { get; } = new ProtocolKnowledge();

        public IObservable<(byte[] data, LegoWirelessMessage message)> UpstreamRawMessages => _upstreamSubject;
        public IObservable<LegoWirelessMessage> UpstreamMessages => _upstreamSubject.Select(x => x.message);
        public IServiceProvider ServiceProvider { get; }

        public LegoWirelessProtocol(BluetoothKernel kernel, ILogger<LegoWirelessProtocol> logger, IDeviceFactory deviceFactory, IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _logger = logger;
            _deviceFactory = deviceFactory ?? throw new ArgumentNullException(nameof(_deviceFactory));
            _upstreamSubject = new Subject<(byte[] data, LegoWirelessMessage message)>();
        }

        public async Task ConnectAsync(SystemType knownSystemType = default)
        {
            // sets initial system type to provided value. This alllows sensitive IPoweredUpDevice to provide the right GetStaticPortInfo (even on initial HubAttachedIO before a HubProperty<SystemType> can be queried).
            await KnowledgeManager.ApplyDynamicProtocolKnowledge(new HubPropertyMessage<SystemType>(HubProperty.SystemTypeId, HubPropertyOperation.Update, knownSystemType)
            {
                HubId = 0x00,
            }, Knowledge, _deviceFactory);

            await _kernel.ConnectAsync();

            await _kernel.ReceiveBytesAsync(async data =>
            {
                try
                {
                    var message = MessageEncoder.Decode(data, Knowledge);

                    await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, Knowledge, _deviceFactory);

                    _upstreamSubject.OnNext((data, message));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in LegoWirelessProtocol Decode/Knowledge");

                    throw;
                }
            });
        }

        public async Task DisconnectAsync()
        {
            await _kernel.DisconnectAsync();
        }

        public async Task SendMessageAsync(LegoWirelessMessage message)
        {
            try
            {
                var data = MessageEncoder.Encode(message, Knowledge);

                await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, Knowledge, _deviceFactory);

                await _kernel.SendBytesAsync(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in LegoWirelessProtocol Encode/Knowledge");

                throw;
            }
        }

        #region Disposable Pattern
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _kernel?.Dispose();
                }
                disposedValue = true;
            }
        }

        // ~LegoWirelessProtocol()
        // {
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}