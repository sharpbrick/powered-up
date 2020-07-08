using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    [DebuggerDisplay("Mode {_modeInfo.HubId}-{_modeInfo.PortId}-{_modeInfo.ModeIndex} {Name}")]
    public class Mode : IDisposable
    {
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private IPoweredUpProtocol _protocol;
        private PortModeInfo _modeInfo;
        protected IObservable<PortValueData> _modeValueObservable;

        public bool IsConnected => (_protocol != null);

        public string Name => _modeInfo.Name;

        public static Mode Create(IPoweredUpProtocol protocol, byte hubId, byte portId, byte modeIndex, IObservable<PortValueData> modeValueObservable)
        {
            var modeInfo = protocol.Knowledge.PortMode(hubId, portId, modeIndex);

            Mode result = null;

            result = (modeInfo.DatasetType, modeInfo.NumberOfDatasets) switch
            {
                (PortModeInformationDataType.SByte, 1) => new SingleValueMode<sbyte>(protocol, modeInfo, modeValueObservable),
                (PortModeInformationDataType.SByte, _) => new MultiValueMode<sbyte>(protocol, modeInfo, modeValueObservable),

                (PortModeInformationDataType.Int16, 1) => new SingleValueMode<short>(protocol, modeInfo, modeValueObservable),
                (PortModeInformationDataType.Int16, _) => new MultiValueMode<short>(protocol, modeInfo, modeValueObservable),

                (PortModeInformationDataType.Int32, 1) => new SingleValueMode<int>(protocol, modeInfo, modeValueObservable),
                (PortModeInformationDataType.Int32, _) => new MultiValueMode<int>(protocol, modeInfo, modeValueObservable),

                (PortModeInformationDataType.Single, 1) => new SingleValueMode<float>(protocol, modeInfo, modeValueObservable),
                (PortModeInformationDataType.Single, _) => new MultiValueMode<float>(protocol, modeInfo, modeValueObservable),

                _ => throw new NotSupportedException("Mode of unknown data type"),
            };

            return result;
        }

        protected Mode(IPoweredUpProtocol protocol, PortModeInfo modeInfo, IObservable<PortValueData> modeValueObservable)
        {
            this._protocol = protocol;
            this._modeInfo = modeInfo;
            this._modeValueObservable = modeValueObservable;
        }

        public async Task<PortFeedback> WriteDirectModeDataAsync(params byte[] data)
        {
            AssertIsConnected();

            var response = await _protocol.SendPortOutputCommandAsync(new GenericWriteDirectModeDataMessage(_modeInfo.ModeIndex)
            {
                HubId = _modeInfo.HubId,
                PortId = _modeInfo.PortId,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                Data = data,
            });

            return response;
        }

        protected void ObserveOnLocalProperty<T>(IObservable<T> modeObservable, params Action<T>[] updaters)
        {
            var disposable = modeObservable.Subscribe(v =>
            {
                foreach (var u in updaters)
                {
                    u(v);
                }
            });

            _compositeDisposable.Add(disposable);
        }

        public async Task SetupNotificationAsync(bool enabled, uint deltaInterval = 5)
        {
            AssertIsConnected();

            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                HubId = _modeInfo.HubId,
                PortId = _modeInfo.PortId,
                Mode = _modeInfo.ModeIndex,
                DeltaInterval = deltaInterval,
                NotificationEnabled = enabled,
            });
        }

        protected void AssertIsConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("The device needs to be connected to a protocol.");
            }
        }

        #region Disposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _compositeDisposable.Dispose();
                }
                disposedValue = true;
            }
        }

        // ~Mode()
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