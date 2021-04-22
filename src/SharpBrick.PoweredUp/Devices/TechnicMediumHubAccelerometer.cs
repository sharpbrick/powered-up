using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    // accelerometers measure translation
    public class TechnicMediumHubAccelerometer : Device, IPoweredUpDevice
    {
        protected MultiValueMode<short, short> _gravityMode;

        public byte ModeIndexGravity { get; protected set; } = 0;
        public byte ModeIndexCalibration { get; protected set; } = 1;

        public (short x, short y, short z) Gravity => (_gravityMode.SI[0], _gravityMode.SI[1], _gravityMode.SI[2]);
        public IObservable<(short x, short y, short z)> GravityObservable => _gravityMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));

        public TechnicMediumHubAccelerometer()
        { }

        public TechnicMediumHubAccelerometer(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _gravityMode = MultiValueMode<short, short>(ModeIndexGravity);

            ObserveForPropertyChanged(_gravityMode.Observable, nameof(Gravity));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-61-01-02-02-03-00-00-00
05-00-43-61-02
11-00-44-61-00-00-47-52-56-00-00-00-00-00-00-00-00
0E-00-44-61-00-01-00-00-00-C7-00-00-00-47
0E-00-44-61-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-61-00-03-00-00-FA-C5-00-00-FA-45
0A-00-44-61-00-04-6D-47-00-00
08-00-44-61-00-05-50-00
0A-00-44-61-00-80-03-01-03-00
11-00-44-61-01-00-43-41-4C-00-00-00-00-00-00-00-00
0E-00-44-61-01-01-00-00-80-3F-00-00-80-3F
0E-00-44-61-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-61-01-03-00-00-80-3F-00-00-80-3F
0A-00-44-61-01-04-00-00-00-00
08-00-44-61-01-05-50-00
0A-00-44-61-01-80-01-00-00-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}