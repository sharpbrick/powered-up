using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp
{
    public class ColorDistanceSensor : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _colorMode;
        protected SingleValueMode<sbyte, sbyte> _proximityMode;
        protected SingleValueMode<int, int> _countMode;
        protected SingleValueMode<sbyte, sbyte> _reflectionMode;
        protected SingleValueMode<sbyte, sbyte> _ambientLightMode;
        protected SingleValueMode<sbyte, sbyte> _lightMode;
        protected MultiValueMode<short, short> _rgbMode;

        public byte ModeIndexColor { get; protected set; } = 0;
        public byte ModeIndexProximity { get; protected set; } = 1;
        public byte ModeIndexCount { get; protected set; } = 2;
        public byte ModeIndexReflection { get; protected set; } = 3;
        public byte ModeIndexAmbientLight { get; protected set; } = 4;
        public byte ModeIndexLight { get; protected set; } = 5;
        public byte ModeIndexRgb { get; protected set; } = 6;
        public byte ModeIndexIRTx { get; protected set; } = 7;
        public byte ModeIndexSPEC1 { get; protected set; } = 8;
        public byte ModeIndexDebug { get; protected set; } = 9;
        public byte ModeIndexCalibration { get; protected set; } = 10;

        public TechnicColor Color => (TechnicColor)_colorMode.SI;
        public IObservable<TechnicColor> ColorObservable => _colorMode.Observable.Select(v => (TechnicColor)v.SI);
        public IObservable<sbyte> ProximityObservable => _proximityMode.Observable.Select(v => v.SI);
        public IObservable<int> CountObservable => _countMode.Observable.Select(v => v.SI);
        public IObservable<sbyte> ReflectionObservable => _reflectionMode.Observable.Select(v => v.SI);
        public IObservable<sbyte> AmbientLightObservable => _ambientLightMode.Observable.Select(v => v.SI);
        public IObservable<(short red, short green, short blue)> RgbObservable => _rgbMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));

        public ColorDistanceSensor()
        { }
        public ColorDistanceSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _colorMode = SingleValueMode<sbyte, sbyte>(ModeIndexColor);
            _proximityMode = SingleValueMode<sbyte, sbyte>(ModeIndexProximity);
            _countMode = SingleValueMode<int, int>(ModeIndexCount);
            _reflectionMode = SingleValueMode<sbyte, sbyte>(ModeIndexReflection);
            _ambientLightMode = SingleValueMode<sbyte, sbyte>(ModeIndexAmbientLight);
            _lightMode = SingleValueMode<sbyte, sbyte>(ModeIndexLight);
            _rgbMode = MultiValueMode<short, short>(ModeIndexRgb);
        }

        protected override uint GetDefaultDeltaInterval(byte modeIndex)
           => modeIndex switch
           {
               0 => 1,
               2 => 1,
               _ => base.GetDefaultDeltaInterval(modeIndex),
           };

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => ((softwareVersion, hardwareVersion, systemType) switch
            {
                (_, _, _) => @"
0B-00-43-01-01-07-0B-5F-06-A0-00
07-00-43-01-02-4F-00
12-00-44-01-00-00-43-4F-4C-4F-52-00-00-00-00-00-00-00
0E-00-44-01-00-01-00-00-00-00-00-00-20-41
0E-00-44-01-00-02-00-00-00-00-00-00-C8-42
0E-00-44-01-00-03-00-00-00-00-00-00-20-41
0B-00-44-01-00-04-49-44-58-00-00
08-00-44-01-00-05-C4-00
0A-00-44-01-00-80-01-00-03-00
12-00-44-01-01-00-50-52-4F-58-00-00-00-00-00-00-00-00
0E-00-44-01-01-01-00-00-00-00-00-00-20-41
0E-00-44-01-01-02-00-00-00-00-00-00-C8-42
0E-00-44-01-01-03-00-00-00-00-00-00-20-41
0B-00-44-01-01-04-44-49-53-00-00
08-00-44-01-01-05-50-00
0A-00-44-01-01-80-01-00-03-00
12-00-44-01-02-00-43-4F-55-4E-54-00-00-00-00-00-00-00
0E-00-44-01-02-01-00-00-00-00-00-00-C8-42
0E-00-44-01-02-02-00-00-00-00-00-00-C8-42
0E-00-44-01-02-03-00-00-00-00-00-00-C8-42
0B-00-44-01-02-04-43-4E-54-00-00
08-00-44-01-02-05-08-00
0A-00-44-01-02-80-01-02-04-00
12-00-44-01-03-00-52-45-46-4C-54-00-00-00-00-00-00-00
0E-00-44-01-03-01-00-00-00-00-00-00-C8-42
0E-00-44-01-03-02-00-00-00-00-00-00-C8-42
0E-00-44-01-03-03-00-00-00-00-00-00-C8-42
0B-00-44-01-03-04-50-43-54-00-00
08-00-44-01-03-05-10-00
0A-00-44-01-03-80-01-00-03-00
12-00-44-01-04-00-41-4D-42-49-00-00-00-00-00-00-00-00
0E-00-44-01-04-01-00-00-00-00-00-00-C8-42
0E-00-44-01-04-02-00-00-00-00-00-00-C8-42
0E-00-44-01-04-03-00-00-00-00-00-00-C8-42
0B-00-44-01-04-04-50-43-54-00-00
08-00-44-01-04-05-10-00
0A-00-44-01-04-80-01-00-03-00
12-00-44-01-05-00-43-4F-4C-20-4F-00-00-00-00-00-00-00
0E-00-44-01-05-01-00-00-00-00-00-00-20-41
0E-00-44-01-05-02-00-00-00-00-00-00-C8-42
0E-00-44-01-05-03-00-00-00-00-00-00-20-41
0B-00-44-01-05-04-49-44-58-00-00
08-00-44-01-05-05-00-04
0A-00-44-01-05-80-01-00-03-00
12-00-44-01-06-00-52-47-42-20-49-00-00-00-00-00-00-00
0E-00-44-01-06-01-00-00-00-00-00-C0-7F-44
0E-00-44-01-06-02-00-00-00-00-00-00-C8-42
0E-00-44-01-06-03-00-00-00-00-00-C0-7F-44
0B-00-44-01-06-04-52-41-57-00-00
08-00-44-01-06-05-10-00
0A-00-44-01-06-80-03-01-05-00
12-00-44-01-07-00-49-52-20-54-78-00-00-00-00-00-00-00
0E-00-44-01-07-01-00-00-00-00-00-FF-7F-47
0E-00-44-01-07-02-00-00-00-00-00-00-C8-42
0E-00-44-01-07-03-00-00-00-00-00-FF-7F-47
0B-00-44-01-07-04-4E-2F-41-00-00
08-00-44-01-07-05-00-04
0A-00-44-01-07-80-01-01-05-00
12-00-44-01-08-00-53-50-45-43-20-31-00-00-00-00-00-00
0E-00-44-01-08-01-00-00-00-00-00-00-7F-43
0E-00-44-01-08-02-00-00-00-00-00-00-C8-42
0E-00-44-01-08-03-00-00-00-00-00-00-7F-43
0B-00-44-01-08-04-4E-2F-41-00-00
08-00-44-01-08-05-00-00
0A-00-44-01-08-80-04-00-03-00
12-00-44-01-09-00-44-45-42-55-47-00-00-00-00-00-00-00
0E-00-44-01-09-01-00-00-00-00-00-C0-7F-44
0E-00-44-01-09-02-00-00-00-00-00-00-C8-42
0E-00-44-01-09-03-00-00-00-00-00-00-20-41
0B-00-44-01-09-04-4E-2F-41-00-00
08-00-44-01-09-05-10-00
0A-00-44-01-09-80-02-01-05-00
12-00-44-01-0A-00-43-41-4C-49-42-00-00-00-00-00-00-00
0E-00-44-01-0A-01-00-00-00-00-00-FF-7F-47
0E-00-44-01-0A-02-00-00-00-00-00-00-C8-42
0E-00-44-01-0A-03-00-00-00-00-00-FF-7F-47
0B-00-44-01-0A-04-4E-2F-41-00-00
08-00-44-01-0A-05-10-00
0A-00-44-01-0A-80-08-01-05-00
"
            }).Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}
