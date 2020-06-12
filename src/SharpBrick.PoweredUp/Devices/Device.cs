using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public abstract class Device : IDisposable
    {
        protected readonly IPoweredUpProtocol _protocol;
        private readonly byte _hubId;
        protected readonly byte _portId;
        private readonly IDisposable _receivingDisposable;

        public bool IsConnected => (_protocol != null);

        public Device()
        { }

        public Device(IPoweredUpProtocol protocol, byte hubId, byte portId)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _hubId = hubId;
            _portId = portId;

            _receivingDisposable = _protocol.UpstreamMessages
                .Where(msg => msg.HubId == _hubId)
                .SelectMany(msg => msg switch
                {
                    PortValueSingleMessage pvsm => pvsm.Data,
                    PortValueCombinedModeMessage pvcmm => pvcmm.Data,
                    _ => Array.Empty<PortValueData>(),
                })
                .Where(pvd => pvd.PortId == _portId)
                .Subscribe(pvd =>
                {
                    OnPortValueChange(pvd);
                });
        }

        public async Task SetupNotificationAsync(byte modeIndex, bool enabled, uint deltaInterval = 5)
        {
            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                PortId = _portId,
                Mode = modeIndex,
                DeltaInterval = deltaInterval,
                NotificationEnabled = enabled,
            });
        }

        protected virtual bool OnPortValueChange(PortValueData portValue)
            => false;

        #region Disposable Pattern
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _receivingDisposable?.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
        // ~Device()
        // {
        //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}