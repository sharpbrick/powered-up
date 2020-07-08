using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract class Device : IDisposable
    {
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();

        protected readonly IPoweredUpProtocol _protocol;
        protected readonly byte _hubId;
        protected readonly byte _portId;
        protected readonly IObservable<PortValueData> _portValueObservable;

        public bool IsConnected => (_protocol != null);

        public Device()
        { }

        public Device(IPoweredUpProtocol protocol, byte hubId, byte portId)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _hubId = hubId;
            _portId = portId;

            _portValueObservable = _protocol.UpstreamMessages
                .Where(msg => msg.HubId == _hubId)
                .SelectMany(msg => msg switch
                {
                    PortValueSingleMessage pvsm => pvsm.Data,
                    PortValueCombinedModeMessage pvcmm => pvcmm.Data,
                    _ => Array.Empty<PortValueData>(),
                })
                .Where(pvd => pvd.PortId == _portId);
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

        protected IObservable<Value<TPayload>> CreateSinglePortModeValueObservable<TPayload>(byte modeIndex)
            => CreatePortModeValueObservable<TPayload, Value<TPayload>>(modeIndex, pvd => new Value<TPayload>()
            {
                Raw = pvd.InputValues[0],
                SI = pvd.SIInputValues[0],
                Pct = pvd.PctInputValues[0],
            });

        protected IObservable<TTarget> CreatePortModeValueObservable<TSource, TTarget>(byte modeIndex, Func<PortValueData<TSource>, TTarget> converter)
            => _portValueObservable
                .Where(pvd => pvd.ModeIndex == modeIndex)
                .Cast<PortValueData<TSource>>()
                .Select(converter);

        public async Task SetupNotificationAsync(byte modeIndex, bool enabled, uint deltaInterval = 5)
        {
            AssertIsConnected();

            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                Mode = modeIndex,
                DeltaInterval = deltaInterval,
                NotificationEnabled = enabled,
            });
        }

        private byte IndexOfSupportedCombinedMode(byte[] modeIndices)
        {
            var portInfo = _protocol.Knowledge.Port(_hubId, _portId);

            byte result = 0xFF;

            // e.g. technic motor: mode combination[0 = idx] = 0b0000_0000_0000_1110 / SPEED (mode 1), POS (mode 2), APOS (mode 3)
            for (byte idx = 0; idx < portInfo.ModeCombinations.Length; idx++)
            {
                var mc = portInfo.ModeCombinations[idx];

                if (modeIndices.All(requestedIndex => ((1 << requestedIndex) & mc) > 0))
                {
                    result = idx;

                    break;
                }
            }

            return result;
        }

        public async Task<bool> TryLockDeviceForCombinedModeNotificationSetupAsync(params byte[] modeIndices)
        {
            AssertIsConnected();

            var result = false;

            var combinationModeIndex = IndexOfSupportedCombinedMode(modeIndices);

            if (combinationModeIndex <= 7) // spec chapter 3.18.1 max combination mode index 
            {
                await _protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeMessage()
                {
                    HubId = _hubId,
                    PortId = _portId,
                    SubCommand = PortInputFormatSetupCombinedSubCommand.LockDeviceForSetup,
                });

                // if this needs to be performed after the port formats, cache it for the unlock function
                await _protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeForSetModeDataSetMessage()
                {
                    HubId = _hubId,
                    PortId = _portId,
                    SubCommand = PortInputFormatSetupCombinedSubCommand.SetModeAndDataSetCombination,

                    CombinationIndex = combinationModeIndex,
                    ModeDataSets = modeIndices.Select(mode => new PortInputFormatSetupCombinedModeModeDataSet() { Mode = mode, DataSet = 0, }).ToArray(), //TODO: manage DataSet for device which has (A) multiple modes and (B) returns for a mode more than one data set (e.g. R, G, B for color).
                });

                result = true;
            }

            return result;
        }

        public async Task UnlockFromCombinedModeNotificationSetupAsync(bool enableUpdates)
        {
            AssertIsConnected();

            await _protocol.SendMessageAsync(new PortInputFormatSetupCombinedModeMessage()
            {
                HubId = _hubId,
                PortId = _portId,
                SubCommand = enableUpdates
                    ? PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateEnabled
                    : PortInputFormatSetupCombinedSubCommand.UnlockAndStartWithMultiUpdateDisabled,
            });
        }

        protected void AssertIsConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("The device needs to be connected to a protocol.");
            }
        }

        protected void AssertIsVirtualPort()
        {
            var port = _protocol.Knowledge.Port(_hubId, _portId);

            if (!(port?.IsVirtual ?? false))
            {
                throw new InvalidOperationException("This operation is not valid on a non-virtual port.");
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
                    _compositeDisposable?.Dispose();
                    _compositeDisposable = null;
                }

                disposedValue = true;
            }
        }

        // ~Device()
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