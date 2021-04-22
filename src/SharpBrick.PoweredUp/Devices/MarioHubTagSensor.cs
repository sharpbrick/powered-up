using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubTagSensor : Device, IPoweredUpDevice
    {
        protected MultiValueMode<short, short> _tagMode;
        protected MultiValueMode<sbyte, sbyte> _rgbMode;

        public byte ModeIndexTag { get; protected set; } = 0;
        public byte ModeIndexRgb { get; protected set; } = 1;


        public MarioBarcode Barcode => (MarioBarcode)_tagMode.SI[0];
        public MarioColor ColorNo => (MarioColor)_tagMode.SI[1];
        public IObservable<MarioBarcode> BarcodeObservable => _tagMode.Observable.Where(x => x.SI[0] != 0).Select(x => (MarioBarcode)x.SI[0]);
        public IObservable<MarioColor> ColorNoObservable => _tagMode.Observable.Where(x => x.SI[1] != 0).Select(x => (MarioColor)x.SI[1]);

        public (byte red, byte green, byte blue) RgbColor => ((byte)_rgbMode.SI[0], (byte)_rgbMode.SI[1], (byte)_rgbMode.SI[2]);
        public IObservable<(byte red, byte green, byte blue)> RgbColorObservable => _rgbMode.Observable.Select(x => ((byte)x.SI[0], (byte)x.SI[1], (byte)x.SI[2]));

        public MarioHubTagSensor()
        { }

        public MarioHubTagSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _tagMode = MultiValueMode<short, short>(ModeIndexTag);
            _rgbMode = MultiValueMode<sbyte, sbyte>(ModeIndexRgb);

            ObserveForPropertyChanged(_tagMode.Observable, nameof(Barcode));
            ObserveForPropertyChanged(_tagMode.Observable, nameof(ColorNo));
            ObserveForPropertyChanged(_rgbMode.Observable, nameof(RgbColor));
        }

        protected override uint GetDefaultDeltaInterval(byte modeIndex)
            => modeIndex switch
            {
                0x00 => 3,
                0x01 => 5,
                _ => 5,
            };

        public void ExtendPortMode(PortModeInfo modeInfo)
        {
            modeInfo.DisablePercentage = true;
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-01-01-06-02-03-00-00-00
07-00-43-01-02-03-00
11-00-44-01-00-00-54-41-47-00-00-00-00-00-00-00-00
0E-00-44-01-00-01-00-00-00-00-00-00-20-41
0E-00-44-01-00-02-00-00-00-00-00-00-C8-42
0E-00-44-01-00-03-00-00-00-00-00-00-20-41
0A-00-44-01-00-04-69-64-78-00
08-00-44-01-00-05-84-00
0A-00-44-01-00-80-02-01-03-00
11-00-44-01-01-00-52-47-42-00-00-00-00-00-00-00-00
0E-00-44-01-01-01-00-00-00-00-00-00-20-41
0E-00-44-01-01-02-00-00-00-00-00-00-C8-42
0E-00-44-01-01-03-00-00-00-00-00-00-20-41
0A-00-44-01-01-04-72-61-77-00
08-00-44-01-01-05-84-00
0A-00-44-01-01-80-03-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}