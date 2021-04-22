using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    // https://github.com/bricklife/LEGO-Mario-Reveng/blob/master/IOType-0x4a.md
    public class MarioHubPants : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _pantsMode;

        public byte ModeIndexPants { get; protected set; } = 0;

        public MarioPants Pants => (MarioPants)_pantsMode.SI;
        public IObservable<MarioPants> PantsObservable => _pantsMode.Observable.Select(x => (MarioPants)x.SI);

        public MarioHubPants()
        { }

        public MarioHubPants(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _pantsMode = SingleValueMode<sbyte, sbyte>(ModeIndexPants);

            ObserveForPropertyChanged(_pantsMode.Observable, nameof(Pants));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-02-01-02-01-01-00-00-00
05-00-43-02-02
11-00-44-02-00-00-50-41-4E-54-00-00-00-00-00-00-00
0E-00-44-02-00-01-00-00-00-00-00-00-7C-42
0E-00-44-02-00-02-00-00-00-00-00-00-C8-42
0E-00-44-02-00-03-00-00-00-00-00-00-7C-42
0A-00-44-02-00-04-69-64-78-00
08-00-44-02-00-05-84-00
0A-00-44-02-00-80-01-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}