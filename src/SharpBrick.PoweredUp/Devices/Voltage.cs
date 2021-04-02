using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class Voltage : Device, IPoweredUpDevice
    {
        protected SingleValueMode<short, short> _voltageLMode;
        protected SingleValueMode<short, short> _voltageSMode;
        public byte ModeIndexVoltageL { get; protected set; } = 0;
        public byte ModeIndexVoltageS { get; protected set; } = 1;

        public short VoltageL => _voltageLMode.SI;
        public short VoltageLPct => _voltageLMode.Pct;
        public IObservable<Value<short, short>> VoltageLObservable => _voltageLMode.Observable;

        public short VoltageS => _voltageSMode.SI;
        public short VoltageSPct => _voltageSMode.Pct;
        public IObservable<Value<short, short>> VoltageSObservable => _voltageSMode.Observable;

        public Voltage()
        { }

        public Voltage(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _voltageLMode = SingleValueMode<short, short>(ModeIndexVoltageL);
            _voltageSMode = SingleValueMode<short, short>(ModeIndexVoltageS);

            ObserveForPropertyChanged(_voltageLMode.Observable, nameof(VoltageL), nameof(VoltageLPct));
            ObserveForPropertyChanged(_voltageSMode.Observable, nameof(VoltageS), nameof(VoltageSPct));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => ((softwareVersion, hardwareVersion, systemType) switch
            {
                (_, _, SystemType.LegoTechnic_MediumHub) => @"
0B-00-43-3C-01-02-02-03-00-00-00
05-00-43-3C-02
11-00-44-3C-00-00-56-4C-54-20-4C-00-00-00-00-00-00
0E-00-44-3C-00-01-00-00-00-00-00-F0-7F-45
0E-00-44-3C-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-00-03-00-00-00-00-00-3C-16-46
0A-00-44-3C-00-04-6D-56-00-00
08-00-44-3C-00-05-10-00
0A-00-44-3C-00-80-01-01-04-00
11-00-44-3C-01-00-56-4C-54-20-53-00-00-00-00-00-00
0E-00-44-3C-01-01-00-00-00-00-00-F0-7F-45
0E-00-44-3C-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-01-03-00-00-00-00-00-3C-16-46
0A-00-44-3C-01-04-6D-56-00-00
08-00-44-3C-01-05-10-00
0A-00-44-3C-01-80-01-01-04-00
",
                (_, _, SystemType.LegoSystem_Mario) => @"
0B-00-43-06-01-02-02-03-00-00-00
05-00-43-06-02
11-00-44-06-00-00-56-4C-54-20-4C-00-00-00-00-00-00
0E-00-44-06-00-01-00-00-00-00-00-10-29-45
0E-00-44-06-00-02-00-00-00-00-00-00-C8-42
0E-00-44-06-00-03-00-00-00-00-00-40-4E-45
0A-00-44-06-00-04-6D-56-00-00
08-00-44-06-00-05-10-00
0A-00-44-06-00-80-01-01-04-00
11-00-44-06-01-00-56-4C-54-20-53-00-00-00-00-00-00
0E-00-44-06-01-01-00-00-00-00-00-10-29-45
0E-00-44-06-01-02-00-00-00-00-00-00-C8-42
0E-00-44-06-01-03-00-00-00-00-00-40-4E-45
0A-00-44-06-01-04-6D-56-00-00
08-00-44-06-01-05-10-00
0A-00-44-06-01-80-01-01-04-00
",
                (_, _, SystemType.LegoSystem_TwoPortHub) => @"
0B-00-43-3C-01-02-02-03-00-00-00
05-00-43-3C-02
12-00-44-3C-00-00-56-4C-54-20-4C-00-00-00-00-00-00-00
0E-00-44-3C-00-01-00-00-00-00-00-50-73-45
0E-00-44-3C-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-00-03-00-00-00-00-00-00-16-46
0B-00-44-3C-00-04-6D-56-00-00-00
08-00-44-3C-00-05-10-00
0A-00-44-3C-00-80-01-01-04-00
12-00-44-3C-01-00-56-4C-54-20-53-00-00-00-00-00-00-00
0E-00-44-3C-01-01-00-00-00-00-00-50-73-45
0E-00-44-3C-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-01-03-00-00-00-00-00-00-16-46
0B-00-44-3C-01-04-6D-56-00-00-00
08-00-44-3C-01-05-10-00
0A-00-44-3C-01-80-01-01-04-00
",
                (_, _, SystemType.LegoSystem_TwoPortHandset) => @"
0B-00-43-3B-01-02-02-03-00-00-00
0C-00-44-3B-00-00-56-4C-54-20-4C-00
0E-00-44-3B-00-01-00-00-00-00-00-00-48-45
0E-00-44-3B-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3B-00-03-00-00-00-00-00-00-C8-45
0A-00-44-3B-00-04-6D-76-00-00
08-00-44-3B-00-05-10-00
0A-00-44-3B-00-80-01-01-04-00
0C-00-44-3B-01-00-56-4C-54-20-53-00
0E-00-44-3B-01-01-00-00-00-00-00-00-48-45
0E-00-44-3B-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3B-01-03-00-00-00-00-00-00-C8-45
0A-00-44-3B-01-04-6D-76-00-00
08-00-44-3B-01-05-10-00
0A-00-44-3B-01-80-01-01-04-00
",
                (_, _, SystemType.LegoDuplo_DuploTrain) => @"
0B-00-43-14-01-02-02-03-00-00-00
05-00-43-14-02
11-00-44-14-00-00-56-4C-54-20-4C-00-00-00-00-00-00
0E-00-44-14-00-01-00-00-00-00-00-70-3E-45
0E-00-44-14-00-02-00-00-00-00-00-00-C8-42
0E-00-44-14-00-03-00-00-00-00-00-00-C8-45
0A-00-44-14-00-04-6D-56-00-00
08-00-44-14-00-05-10-00
0A-00-44-14-00-80-01-01-04-00
11-00-44-14-01-00-56-4C-54-20-53-00-00-00-00-00-00
0E-00-44-14-01-01-00-00-00-00-00-70-3E-45
0E-00-44-14-01-02-00-00-00-00-00-00-C8-42
0E-00-44-14-01-03-00-00-00-00-00-00-C8-45
0A-00-44-14-01-04-6D-56-00-00
08-00-44-14-01-05-10-00
0A-00-44-14-01-80-01-01-04-00
",
                (_, _, SystemType.LegoSystem_MoveHub) => @"
0B-00-43-3C-01-02-02-03-00-00-00
05-00-43-3C-02
11-00-44-3C-00-00-56-4C-54-20-4C-00-00-00-00-00-00
0E-00-44-3C-00-01-00-00-00-00-00-50-73-45
0E-00-44-3C-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-00-03-00-00-00-00-00-00-16-46
0A-00-44-3C-00-04-6D-56-00-00
08-00-44-3C-00-05-10-00
0A-00-44-3C-00-80-01-01-04-00
11-00-44-3C-01-00-56-4C-54-20-53-00-00-00-00-00-00
0E-00-44-3C-01-01-00-00-00-00-00-50-73-45
0E-00-44-3C-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-01-03-00-00-00-00-00-00-16-46
0A-00-44-3C-01-04-6D-56-00-00
08-00-44-3C-01-05-10-00
0A-00-44-3C-01-80-01-01-04-00
",
                _ => throw new NotImplementedException(),
            }).Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}