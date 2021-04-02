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
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly ILegoWirelessProtocol _protocol;
        private readonly PortModeInfo _modeInfo;
        protected IObservable<PortValueData> _modeValueObservable;

        public bool IsConnected => (_protocol is not null);

        public string Name => _modeInfo.Name;
        public string Symbol => _modeInfo.Symbol;

        public static Mode Create(ILegoWirelessProtocol protocol, byte hubId, byte portId, byte modeIndex, IObservable<PortValueData> modeValueObservable)
        {
            var modeInfo = protocol.Knowledge.PortMode(hubId, portId, modeIndex);

            Mode result = modeInfo switch
            {
                { DatasetType: PortModeInformationDataType.SByte, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: false } => new SingleValueMode<sbyte, sbyte>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.SByte, NumberOfDatasets: _, OverrideDatasetTypeToDouble: false } => new MultiValueMode<sbyte, sbyte>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Int16, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: false } => new SingleValueMode<short, short>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Int16, NumberOfDatasets: _, OverrideDatasetTypeToDouble: false } => new MultiValueMode<short, short>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Int32, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: false } => new SingleValueMode<int, int>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Int32, NumberOfDatasets: _, OverrideDatasetTypeToDouble: false } => new MultiValueMode<int, int>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Single, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: false } => new SingleValueMode<float, float>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Single, NumberOfDatasets: _, OverrideDatasetTypeToDouble: false } => new MultiValueMode<float, float>(protocol, modeInfo, modeValueObservable),

                // override to allow scaling crossing the boundary of the original data type
                { DatasetType: PortModeInformationDataType.SByte, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: true } => new SingleValueMode<sbyte, double>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.SByte, NumberOfDatasets: _, OverrideDatasetTypeToDouble: true } => new MultiValueMode<sbyte, double>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Int16, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: true } => new SingleValueMode<short, double>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Int16, NumberOfDatasets: _, OverrideDatasetTypeToDouble: true } => new MultiValueMode<short, double>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Int32, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: true } => new SingleValueMode<int, double>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Int32, NumberOfDatasets: _, OverrideDatasetTypeToDouble: true } => new MultiValueMode<int, double>(protocol, modeInfo, modeValueObservable),

                { DatasetType: PortModeInformationDataType.Single, NumberOfDatasets: 1, OverrideDatasetTypeToDouble: true } => new SingleValueMode<float, double>(protocol, modeInfo, modeValueObservable),
                { DatasetType: PortModeInformationDataType.Single, NumberOfDatasets: _, OverrideDatasetTypeToDouble: true } => new MultiValueMode<float, double>(protocol, modeInfo, modeValueObservable),

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