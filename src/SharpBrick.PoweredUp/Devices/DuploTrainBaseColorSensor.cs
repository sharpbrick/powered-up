using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class DuploTrainBaseColorSensor : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _colorMode;
        protected SingleValueMode<sbyte, sbyte> _colorTagMode;
        protected SingleValueMode<sbyte, sbyte> _reflectionMode;
        protected MultiValueMode<short, short> _rgbMode;

        public byte ModeIndexColor { get; protected set; } = 0;
        public byte ModeIndexColorTag { get; protected set; } = 1;
        public byte ModeIndexReflection { get; protected set; } = 2;
        public byte ModeIndexRgb { get; protected set; } = 3;

        public sbyte Color => _colorMode.SI;
        public DuploColorTag ColorTag => (DuploColorTag)_colorTagMode.SI;
        public sbyte Reflection => _reflectionMode.SI;
        public (short red, short green, short blue) Rgb => (_rgbMode.SI[0], _rgbMode.SI[1], _rgbMode.SI[2]);
        public (short red, short green, short blue) RgbPct => (_rgbMode.Pct[0], _rgbMode.Pct[1], _rgbMode.Pct[2]);

        public IObservable<sbyte> ColorObservable => _colorMode.Observable.Select(v => v.SI);
        public IObservable<DuploColorTag> ColorTagObservable => _colorTagMode.Observable.Select(v => (DuploColorTag)v.SI);
        public IObservable<sbyte> ReflectionObservable => _reflectionMode.Observable.Select(v => v.SI);
        public IObservable<(short red, short green, short blue)> RgbObservable => _rgbMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));
        public IObservable<(short red, short green, short blue)> RgbPctObservable => _rgbMode.Observable.Select(v => (v.Pct[0], v.Pct[1], v.Pct[2]));

        public DuploTrainBaseColorSensor()
        { }

        public DuploTrainBaseColorSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _colorMode = SingleValueMode<sbyte, sbyte>(ModeIndexColor);
            _colorTagMode = SingleValueMode<sbyte, sbyte>(ModeIndexColorTag);
            _reflectionMode = SingleValueMode<sbyte, sbyte>(ModeIndexReflection);
            _rgbMode = MultiValueMode<short, short>(ModeIndexRgb);

            ObserveForPropertyChanged(_colorMode.Observable, nameof(Color));
            ObserveForPropertyChanged(_colorTagMode.Observable, nameof(ColorTag));
            ObserveForPropertyChanged(_reflectionMode.Observable, nameof(Reflection));
            ObserveForPropertyChanged(_rgbMode.Observable, nameof(Rgb), nameof(RgbPct));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-12-01-06-05-1F-00-00-00
07-00-43-12-02-0F-00
11-00-44-12-00-00-43-4F-4C-4F-52-00-00-00-00-00-00
0E-00-44-12-00-01-00-00-00-00-00-00-20-41
0E-00-44-12-00-02-00-00-00-00-00-00-C8-42
0E-00-44-12-00-03-00-00-00-00-00-00-20-41
0A-00-44-12-00-04-69-64-78-00
08-00-44-12-00-05-84-00
0A-00-44-12-00-80-01-00-03-00
11-00-44-12-01-00-43-20-54-41-47-00-00-00-00-00-00
0E-00-44-12-01-01-00-00-00-00-00-00-20-41
0E-00-44-12-01-02-00-00-00-00-00-00-C8-42
0E-00-44-12-01-03-00-00-00-00-00-00-20-41
0A-00-44-12-01-04-69-64-78-00
08-00-44-12-01-05-C4-00
0A-00-44-12-01-80-01-00-03-00
11-00-44-12-02-00-52-45-46-4C-54-00-00-00-00-00-00
0E-00-44-12-02-01-00-00-00-00-00-00-C8-42
0E-00-44-12-02-02-00-00-00-00-00-00-C8-42
0E-00-44-12-02-03-00-00-00-00-00-00-C8-42
0A-00-44-12-02-04-72-61-77-00
08-00-44-12-02-05-10-00
0A-00-44-12-02-80-01-00-03-00
11-00-44-12-03-00-52-47-42-20-49-00-00-00-00-00-00
0E-00-44-12-03-01-00-00-00-00-00-C0-7F-44
0E-00-44-12-03-02-00-00-00-00-00-00-C8-42
0E-00-44-12-03-03-00-00-00-00-00-C0-7F-44
0A-00-44-12-03-04-72-61-77-00
08-00-44-12-03-05-10-00
0A-00-44-12-03-80-03-01-05-00
11-00-44-12-04-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-12-04-01-00-00-00-00-00-00-FA-45
0E-00-44-12-04-02-00-00-00-00-00-00-C8-42
0E-00-44-12-04-03-00-00-00-00-00-00-FA-45
0A-00-44-12-04-04-00-00-00-00
08-00-44-12-04-05-10-00
0A-00-44-12-04-80-03-01-05-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}