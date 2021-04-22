using System;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public class MultiValueMode<TDatasetType, TOutputType> : Mode
    {
        public TDatasetType[] Raw { get; private set; }
        public TOutputType[] SI { get; private set; }
        public TOutputType[] Pct { get; private set; }

        public IObservable<Value<TDatasetType[], TOutputType[]>> Observable { get; }

        public MultiValueMode(ILegoWirelessProtocol protocol, PortModeInfo modeInfo, IObservable<PortValueData> modeValueObservable)
                : base(protocol, modeInfo, modeValueObservable)
        {
            Observable = CreateObservable();
            ObserveOnLocalProperty(Observable, v => Raw = v.Raw, v => SI = v.SI, v => Pct = v.Pct);
            ObserveForPropertyChanged(Observable, nameof(Raw), nameof(SI), nameof(Pct));
        }
        protected IObservable<Value<TDatasetType[], TOutputType[]>> CreateObservable()
            => _modeValueObservable
                .Cast<PortValueData<TDatasetType, TOutputType>>()
                .Select(pvd => new Value<TDatasetType[], TOutputType[]>()
                {
                    Raw = pvd.InputValues,
                    SI = pvd.SIInputValues,
                    Pct = pvd.PctInputValues,
                });
    }
}