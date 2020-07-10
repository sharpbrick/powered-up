using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicMediumHubTemperatureSensor : Device, IPoweredUpDevice
    {
        protected SingleValueMode<short> _temperatureMode;
        public byte ModeIndexTemperature { get; protected set; } = 0;

        public short Temperature => _temperatureMode.SI;
        public short TemperaturePct => _temperatureMode.Pct;
        public IObservable<Value<short>> TemperatureObservable => _temperatureMode.Observable;

        public TechnicMediumHubTemperatureSensor()
        { }

        public TechnicMediumHubTemperatureSensor(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _temperatureMode = SingleValueMode<short>(ModeIndexTemperature);
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
0B-00-43-60-01-02-01-01-00-00-00
05-00-43-60-02
11-00-44-60-00-00-54-45-4D-50-00-00-00-00-00-00-00
0E-00-44-60-00-01-00-00-61-C4-00-00-61-44
0E-00-44-60-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-60-00-03-00-00-B4-C2-00-00-B4-42
0A-00-44-60-00-04-44-45-47-00
08-00-44-60-00-05-50-00
0A-00-44-60-00-80-01-01-05-01
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}