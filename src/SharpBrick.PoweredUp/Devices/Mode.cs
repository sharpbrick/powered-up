using System;
using System.ComponentModel;
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
    public class Mode : IDisposable, INotifyPropertyChanged
    {
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private ILegoWirelessProtocol _protocol;
        private PortModeInfo _modeInfo;
        protected IObservable<PortValueData> _modeValueObservable;

        public bool IsConnected => (_protocol != null);

        public string Name => _modeInfo.Name;
        public string Symbol => _modeInfo.Symbol;

        public static Mode Create(ILegoWirelessProtocol protocol, byte hubId, byte portId, byte modeIndex, IObservable<PortValueData> modeValueObservable)
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

        protected Mode(ILegoWirelessProtocol protocol, PortModeInfo modeInfo, IObservable<PortValueData> modeValueObservable)
        {
            this._protocol = protocol;
            this._modeInfo = modeInfo;
            this._modeValueObservable = modeValueObservable;
        }

        public async Task<PortFeedback> WriteDirectModeDataAsync(params byte[] data)
        {
            AssertIsConnected();

            if (_modeInfo.IsOutput == false)
            {
                throw new InvalidOperationException("The protocol knowledge declares that this mode cannot be written to (IsOutput = false)");
            }

            var response = await _protocol.SendPortOutputCommandAsync(new GenericWriteDirectModeDataMessage(
                _modeInfo.PortId,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                _modeInfo.ModeIndex,
                data)
            {
                HubId = _modeInfo.HubId,
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

            if (_modeInfo.IsInput == false)
            {
                throw new InvalidOperationException("The protocol knowledge declares that this mode cannot be read (IsInput = false)");
            }

            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage(_modeInfo.PortId, _modeInfo.ModeIndex, deltaInterval, enabled)
            {
                HubId = _modeInfo.HubId,
            });
        }

        protected void AssertIsConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("The device needs to be connected to a protocol.");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void ObserveForPropertyChanged<T>(IObservable<T> observable, params string[] propertyNames)
        {
            _compositeDisposable.Add(observable.Subscribe(_ =>
            {
                foreach (var propertyName in propertyNames)
                {
                    OnPropertyChanged(propertyName);
                }
            }));
        }
        #endregion

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