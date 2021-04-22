using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicColorSensor : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _colorMode;
        protected SingleValueMode<sbyte, sbyte> _reflectionMode;
        protected SingleValueMode<sbyte, sbyte> _ambientLightMode;
        protected MultiValueMode<sbyte, sbyte> _lightMode;

        protected MultiValueMode<short, short> _rawReflectionMode;
        protected MultiValueMode<short, short> _rgbMode;
        protected MultiValueMode<short, short> _hsvMode;
        protected MultiValueMode<short, short> _shsvMode;

        public byte ModeIndexColor { get; protected set; } = 0;
        public byte ModeIndexReflection { get; protected set; } = 1;
        public byte ModeIndexAmbientLight { get; protected set; } = 2;
        public byte ModeIndexLight { get; protected set; } = 3;
        public byte ModeIndexRawReflection { get; protected set; } = 4;
        public byte ModeIndexRgbIntern { get; protected set; } = 5;
        public byte ModeIndexHSV { get; protected set; } = 6;
        public byte ModeIndexSHSV { get; protected set; } = 7;
        public byte ModeIndexDebug { get; protected set; } = 8;
        public byte ModeIndexCalibration { get; protected set; } = 9;


        public sbyte Color => _colorMode.SI;
        public IObservable<TechnicColor> ColorObservable => _colorMode.Observable.Select(v => (TechnicColor)v.SI);
        public IObservable<sbyte> ReflectionObservable => _reflectionMode.Observable.Select(v => v.SI);
        public IObservable<sbyte> AmbientLightObservable => _ambientLightMode.Observable.Select(v => v.SI);
        public IObservable<(short red, short green, short blue, short fourthValue)> RgbObservable => _rgbMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2], v.SI[3]));
        public IObservable<(short hue, short saturation, short value)> HsvObservable => _hsvMode.Observable.Select(v => (v.SI[0], v.SI[1], v.SI[2]));


        public (short red, short green, short blue) Rgb => (_rgbMode.SI[0], _rgbMode.SI[1], _rgbMode.SI[2]);

        public TechnicColorSensor()
        { }
        public TechnicColorSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _colorMode = SingleValueMode<sbyte, sbyte>(ModeIndexColor);
            _reflectionMode = SingleValueMode<sbyte, sbyte>(ModeIndexReflection);
            _ambientLightMode = SingleValueMode<sbyte, sbyte>(ModeIndexAmbientLight);
            _lightMode = MultiValueMode<sbyte, sbyte>(ModeIndexLight);
            _rgbMode = MultiValueMode<short, short>(ModeIndexRgbIntern);
            _hsvMode = MultiValueMode<short, short>(ModeIndexHSV);
        }

        public async Task SetSectorLightAsync(byte sector1, byte sector2, byte sector3)
        {
            AssertIsConnected();

            await _lightMode.WriteDirectModeDataAsync(new byte[] { sector1, sector2, sector3 });
            await _protocol.SendMessageAsync(new PortInputFormatSetupSingleMessage(_portId, ModeIndexLight, 10000, false)
            {
                HubId = _hubId,
            });
        }

        protected override uint GetDefaultDeltaInterval(byte modeIndex)
            => modeIndex switch
            {
                1 => 1,
                2 => 1,
                _ => base.GetDefaultDeltaInterval(modeIndex),
            };

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
                => ((softwareVersion, hardwareVersion, systemType) switch
                {
                    (_, _, _) => @"
0B-00-43-01-01-07-0A-F7-01-08-00
07-00-43-01-02-63-00
11-00-44-01-00-00-43-4F-4C-4F-52-00-00-00-00-00-00
0E-00-44-01-00-01-00-00-00-00-00-00-20-41
0E-00-44-01-00-02-00-00-00-00-00-00-C8-42
0E-00-44-01-00-03-00-00-00-00-00-00-20-41
0A-00-44-01-00-04-49-44-58-00
08-00-44-01-00-05-E4-00
0A-00-44-01-00-80-01-00-02-00
11-00-44-01-01-00-52-45-46-4C-54-00-00-00-00-00-00
0E-00-44-01-01-01-00-00-00-00-00-00-C8-42
0E-00-44-01-01-02-00-00-00-00-00-00-C8-42
0E-00-44-01-01-03-00-00-00-00-00-00-C8-42
0A-00-44-01-01-04-50-43-54-00
08-00-44-01-01-05-30-00
0A-00-44-01-01-80-01-00-03-00
11-00-44-01-02-00-41-4D-42-49-00-00-00-00-00-00-00
0E-00-44-01-02-01-00-00-00-00-00-00-C8-42
0E-00-44-01-02-02-00-00-00-00-00-00-C8-42
0E-00-44-01-02-03-00-00-00-00-00-00-C8-42
0A-00-44-01-02-04-50-43-54-00
08-00-44-01-02-05-30-00
0A-00-44-01-02-80-01-00-03-00
11-00-44-01-03-00-4C-49-47-48-54-00-00-00-00-00-00
0E-00-44-01-03-01-00-00-00-00-00-00-C8-42
0E-00-44-01-03-02-00-00-00-00-00-00-C8-42
0E-00-44-01-03-03-00-00-00-00-00-00-C8-42
0A-00-44-01-03-04-50-43-54-00
08-00-44-01-03-05-00-10
0A-00-44-01-03-80-03-00-03-00
11-00-44-01-04-00-52-52-45-46-4C-00-00-00-00-00-00
0E-00-44-01-04-01-00-00-00-00-00-00-80-44
0E-00-44-01-04-02-00-00-00-00-00-00-C8-42
0E-00-44-01-04-03-00-00-00-00-00-00-80-44
0A-00-44-01-04-04-52-41-57-00
08-00-44-01-04-05-10-00
0A-00-44-01-04-80-02-01-04-00
11-00-44-01-05-00-52-47-42-20-49-00-00-00-00-00-00
0E-00-44-01-05-01-00-00-00-00-00-00-80-44
0E-00-44-01-05-02-00-00-00-00-00-00-C8-42
0E-00-44-01-05-03-00-00-00-00-00-00-80-44
0A-00-44-01-05-04-52-41-57-00
08-00-44-01-05-05-10-00
0A-00-44-01-05-80-04-01-04-00
11-00-44-01-06-00-48-53-56-00-00-00-00-00-00-00-00
0E-00-44-01-06-01-00-00-00-00-00-00-B4-43
0E-00-44-01-06-02-00-00-00-00-00-00-C8-42
0E-00-44-01-06-03-00-00-00-00-00-00-B4-43
0A-00-44-01-06-04-52-41-57-00
08-00-44-01-06-05-10-00
0A-00-44-01-06-80-03-01-04-00
11-00-44-01-07-00-53-48-53-56-00-00-00-00-00-00-00
0E-00-44-01-07-01-00-00-00-00-00-00-B4-43
0E-00-44-01-07-02-00-00-00-00-00-00-C8-42
0E-00-44-01-07-03-00-00-00-00-00-00-B4-43
0A-00-44-01-07-04-52-41-57-00
08-00-44-01-07-05-10-00
0A-00-44-01-07-80-04-01-04-00
11-00-44-01-08-00-44-45-42-55-47-00-00-00-00-00-00
0E-00-44-01-08-01-00-00-00-00-00-FF-7F-47
0E-00-44-01-08-02-00-00-00-00-00-00-C8-42
0E-00-44-01-08-03-00-00-00-00-00-FF-7F-47
0A-00-44-01-08-04-52-41-57-00
08-00-44-01-08-05-10-00
0A-00-44-01-08-80-04-01-04-00
11-00-44-01-09-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-01-09-01-00-00-00-00-00-FF-7F-47
0E-00-44-01-09-02-00-00-00-00-00-00-C8-42
0E-00-44-01-09-03-00-00-00-00-00-FF-7F-47
0A-00-44-01-09-04-00-00-00-00
08-00-44-01-09-05-00-00
0A-00-44-01-09-80-07-01-05-00
"
                }).Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}