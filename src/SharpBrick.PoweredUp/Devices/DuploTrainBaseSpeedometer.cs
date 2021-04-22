using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class DuploTrainBaseSpeedometer : Device, IPoweredUpDevice
    {
        protected SingleValueMode<short, short> _speedMode;
        protected SingleValueMode<int, int> _countMode;

        public byte ModeIndexSpeed { get; protected set; } = 0;
        public byte ModeIndexCount { get; protected set; } = 1;

        public short Speed => _speedMode.SI;
        public short SpeedPct => _speedMode.Pct;
        public IObservable<Value<short, short>> SpeedObservable => _speedMode.Observable;

        public int Count => _countMode.SI;
        public IObservable<int> CountObservable => _countMode.Observable.Select(v => v.SI);

        public DuploTrainBaseSpeedometer()
        { }

        public DuploTrainBaseSpeedometer(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _speedMode = SingleValueMode<short, short>(ModeIndexSpeed);
            _countMode = SingleValueMode<int, int>(ModeIndexCount);

            ObserveForPropertyChanged(_speedMode.Observable, nameof(Speed), nameof(SpeedPct));
            ObserveForPropertyChanged(_countMode.Observable, nameof(Count));
        }

        public void ExtendPortMode(PortModeInfo portModeInfo)
        {
            if (portModeInfo.ModeIndex == ModeIndexCount)
            {
                portModeInfo.DisableScaling = true;
                portModeInfo.DisablePercentage = true;
            }
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-13-01-06-03-07-00-00-00
07-00-43-13-02-03-00
11-00-44-13-00-00-53-50-45-45-44-00-00-00-00-00-00
0E-00-44-13-00-01-00-00-96-C3-00-00-96-43
0E-00-44-13-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-13-00-03-00-00-20-C1-00-00-20-41
0A-00-44-13-00-04-73-70-64-00
08-00-44-13-00-05-50-00
0A-00-44-13-00-80-01-01-05-00
11-00-44-13-01-00-43-4F-55-4E-54-00-00-00-00-00-00
0E-00-44-13-01-01-00-00-00-41-00-00-00-41
0E-00-44-13-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-13-01-03-00-00-B4-43-00-00-B4-43
0A-00-44-13-01-04-63-6E-74-00
08-00-44-13-01-05-08-00
0A-00-44-13-01-80-01-02-04-00
11-00-44-13-02-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-13-02-01-00-00-00-00-00-00-80-45
0E-00-44-13-02-02-00-00-00-00-00-00-C8-42
0E-00-44-13-02-03-00-00-00-00-00-00-80-45
0A-00-44-13-02-04-6E-2F-61-00
08-00-44-13-02-05-08-00
0A-00-44-13-02-80-04-01-05-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}