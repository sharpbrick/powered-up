using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class Voltage : Device, IPoweredUpDevice
    {
        public byte ModeIndexVoltageL { get; protected set; } = 0;
        public byte ModeIndexVoltageS { get; protected set; } = 1;

        public short VoltageL { get; private set; }
        public short VoltageLPct { get; private set; }
        public IObservable<Value<short>> VoltageLObservable { get; }

        public short VoltageS { get; private set; }
        public short VoltageSPct { get; private set; }
        public IObservable<Value<short>> VoltageSObservable { get; }

        public Voltage()
        { }

        public Voltage(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            VoltageLObservable = CreateSinglePortModeValueObservable<short>(ModeIndexVoltageL);
            VoltageSObservable = CreateSinglePortModeValueObservable<short>(ModeIndexVoltageS);

            ObserveOnLocalProperty(VoltageLObservable, v => VoltageL = v.SI, v => VoltageLPct = v.Pct);
            ObserveOnLocalProperty(VoltageSObservable, v => VoltageS = v.SI, v => VoltageSPct = v.Pct);
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
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
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}