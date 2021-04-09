using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicDistanceSensor : Device, IPoweredUpDevice
    {
        protected SingleValueMode<short> _distlMode;
        protected SingleValueMode<short> _distsMode;
        protected SingleValueMode<short> _singlMode;
        protected MultiValueMode<sbyte> _lightMode;

        public byte ModeIndexDistance { get; protected set; } = 0;
        public byte ModeIndexShortOnlyDistance { get; protected set; } = 1;
        public byte ModeIndexSingleMeasurement { get; protected set; } = 2;
        public byte ModeIndexLight { get; protected set; } = 5;

        public short Distance => _distlMode.SI;
        public IObservable<short> DistanceObservable => _distlMode.Observable.Select(x => x.SI);
        public short ShortOnlyDistance => _distsMode.SI;
        public IObservable<short> ShortOnlyDistanceObservable => _distsMode.Observable.Select(x => x.SI);
        public short Single => _singlMode.SI;
        public IObservable<short> SingleObservable => _singlMode.Observable.Select(x => x.SI);

        public TechnicDistanceSensor()
        { }
        public TechnicDistanceSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _distlMode = SingleValueMode<short>(ModeIndexDistance);
            _distsMode = SingleValueMode<short>(ModeIndexShortOnlyDistance);
            _singlMode = SingleValueMode<short>(ModeIndexSingleMeasurement);
            _lightMode = MultiValueMode<sbyte>(ModeIndexLight);

            ObserveForPropertyChanged(_distlMode.Observable, nameof(Distance));
            ObserveForPropertyChanged(_distsMode.Observable, nameof(ShortOnlyDistance));
            ObserveForPropertyChanged(_singlMode.Observable, nameof(Single));
        }

        public Task SetEyeLightAsync(byte leftTop = 0x00, byte rightTop = 0x00, byte leftBottom = 0x00, byte rightBottom = 0x00)
            => _lightMode.WriteDirectModeDataAsync(new byte[] { leftTop, rightTop, leftBottom, rightBottom });

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => ((softwareVersion, hardwareVersion, systemType) switch
            {
                (_, _, _) => @"
0B-00-43-00-01-03-09-9F-00-60-00
05-00-43-00-02
11-00-44-00-00-00-44-49-53-54-4C-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-00-00-00-40-1C-45
0E-00-44-00-00-02-00-00-00-00-00-00-C8-42
0E-00-44-00-00-03-00-00-00-00-00-00-7A-43
0A-00-44-00-00-04-43-4D-00-00
08-00-44-00-00-05-91-00
0A-00-44-00-00-80-01-01-05-01
11-00-44-00-01-00-44-49-53-54-53-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-00-00-00-00-A0-43
0E-00-44-00-01-02-00-00-00-00-00-00-C8-42
0E-00-44-00-01-03-00-00-00-00-00-00-00-42
0A-00-44-00-01-04-43-4D-00-00
08-00-44-00-01-05-F1-00
0A-00-44-00-01-80-01-01-04-01
11-00-44-00-02-00-53-49-4E-47-4C-00-00-00-00-00-00
0E-00-44-00-02-01-00-00-00-00-00-40-1C-45
0E-00-44-00-02-02-00-00-00-00-00-00-C8-42
0E-00-44-00-02-03-00-00-00-00-00-00-7A-43
0A-00-44-00-02-04-43-4D-00-00
08-00-44-00-02-05-90-00
0A-00-44-00-02-80-01-01-05-01
11-00-44-00-03-00-4C-49-53-54-4E-00-00-00-00-00-00
0E-00-44-00-03-01-00-00-00-00-00-00-80-3F
0E-00-44-00-03-02-00-00-00-00-00-00-C8-42
0E-00-44-00-03-03-00-00-00-00-00-00-80-3F
0A-00-44-00-03-04-53-54-00-00
08-00-44-00-03-05-10-00
0A-00-44-00-03-80-01-00-01-00
11-00-44-00-04-00-54-52-41-57-00-00-00-00-00-00-00
0E-00-44-00-04-01-00-00-00-00-00-C4-63-46
0E-00-44-00-04-02-00-00-00-00-00-00-C8-42
0E-00-44-00-04-03-00-00-00-00-00-C4-63-46
0A-00-44-00-04-04-75-53-00-00
08-00-44-00-04-05-90-00
0A-00-44-00-04-80-01-02-05-00
11-00-44-00-05-00-4C-49-47-48-54-00-00-00-00-00-00
0E-00-44-00-05-01-00-00-00-00-00-00-C8-42
0E-00-44-00-05-02-00-00-00-00-00-00-C8-42
0E-00-44-00-05-03-00-00-00-00-00-00-C8-42
0A-00-44-00-05-04-50-43-54-00
08-00-44-00-05-05-00-10
0A-00-44-00-05-80-04-00-03-00
11-00-44-00-06-00-50-49-4E-47-00-00-00-00-00-00-00
0E-00-44-00-06-01-00-00-00-00-00-00-80-3F
0E-00-44-00-06-02-00-00-00-00-00-00-C8-42
0E-00-44-00-06-03-00-00-00-00-00-00-80-3F
0A-00-44-00-06-04-50-43-54-00
08-00-44-00-06-05-00-90
0A-00-44-00-06-80-01-00-01-00
11-00-44-00-07-00-41-44-52-41-57-00-00-00-00-00-00
0E-00-44-00-07-01-00-00-00-00-00-00-80-44
0E-00-44-00-07-02-00-00-00-00-00-00-C8-42
0E-00-44-00-07-03-00-00-00-00-00-00-80-44
0A-00-44-00-07-04-50-43-54-00
08-00-44-00-07-05-90-00
0A-00-44-00-07-80-01-01-04-00
11-00-44-00-08-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-00-08-01-00-00-00-00-00-00-7F-43
0E-00-44-00-08-02-00-00-00-00-00-00-C8-42
0E-00-44-00-08-03-00-00-00-00-00-00-7F-43
0A-00-44-00-08-04-50-43-54-00
08-00-44-00-08-05-00-00
0A-00-44-00-08-80-07-00-03-00
"
            }).Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}