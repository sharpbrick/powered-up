using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubAccelerometer : Device, IPoweredUpDevice
    {
        protected MultiValueMode<sbyte, sbyte> _rawMode;
        protected MultiValueMode<short, short> _gestMode;

        public byte ModeIndexRaw { get; protected set; } = 0;
        public byte ModeIndexGesture { get; protected set; } = 1;

        public IObservable<sbyte[]> RawObservable => _rawMode.Observable.Select(x => x.SI);
        public IObservable<MarioGestures[]> GestureObservable => _gestMode.Observable
            .Where(x => x.SI[0] != 0 && x.SI[1] != 0) // filter out none values
            .Select(x => x.SI.Cast<MarioGestures>().ToArray());

        public MarioHubAccelerometer()
        { }

        public MarioHubAccelerometer(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _rawMode = MultiValueMode<sbyte, sbyte>(ModeIndexRaw);
            _gestMode = MultiValueMode<short, short>(ModeIndexGesture);

            //ObserveForPropertyChanged(_rawMode.Observable, nameof(Coins));
        }

        protected override uint GetDefaultDeltaInterval(byte modeIndex)
            => modeIndex switch
            {
                1 => 1,
                _ => 5,
            };

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-00-01-06-02-03-00-00-00
07-00-43-00-02-03-00
11-00-44-00-00-00-52-41-57-00-00-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-00-00-00-00-C8-42
0E-00-44-00-00-02-00-00-00-00-00-00-C8-42
0E-00-44-00-00-03-00-00-00-00-00-00-C8-42
0A-00-44-00-00-04-63-6E-74-00
08-00-44-00-00-05-84-00
0A-00-44-00-00-80-03-00-03-00
11-00-44-00-01-00-47-45-53-54-00-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-00-00-00-00-C8-42
0E-00-44-00-01-02-00-00-00-00-00-00-C8-42
0E-00-44-00-01-03-00-00-00-00-00-00-C8-42
0A-00-44-00-01-04-63-6E-74-00
08-00-44-00-01-05-84-00
0A-00-44-00-01-80-02-01-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}