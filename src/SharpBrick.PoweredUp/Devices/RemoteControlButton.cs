using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace SharpBrick.PoweredUp.Devices
{
    public class RemoteControlButton : Device, IPoweredUpDevice
    {
        protected sbyte PlusBitMask = 0b0000_0001;
        protected sbyte RedBitMask = 0b0000_0010;
        protected sbyte MinusBitMask = 0b0000_0100;

        protected SingleValueMode<sbyte, sbyte> _keyBitFieldMode;
        public byte ModeIndexBitField { get; protected set; } = 3;

        public bool Plus => IsBitMaskSet(_keyBitFieldMode.SI, PlusBitMask);
        public bool Red => IsBitMaskSet(_keyBitFieldMode.SI, RedBitMask);
        public bool Minus => IsBitMaskSet(_keyBitFieldMode.SI, MinusBitMask);

        public IObservable<(bool Plus, bool Stop, bool Minus)> ButtonsObservable => _keyBitFieldMode.Observable.Select(v => (IsBitMaskSet(_keyBitFieldMode.SI, PlusBitMask), IsBitMaskSet(_keyBitFieldMode.SI, RedBitMask), IsBitMaskSet(_keyBitFieldMode.SI, MinusBitMask)));
        public IObservable<bool> PlusObservable => _keyBitFieldMode.Observable.Select(v => IsBitMaskSet(_keyBitFieldMode.SI, PlusBitMask)).DistinctUntilChanged();
        public IObservable<bool> RedObservable => _keyBitFieldMode.Observable.Select(v => IsBitMaskSet(_keyBitFieldMode.SI, RedBitMask)).DistinctUntilChanged();
        public IObservable<bool> MinusObservable => _keyBitFieldMode.Observable.Select(v => IsBitMaskSet(_keyBitFieldMode.SI, MinusBitMask)).DistinctUntilChanged();

        public RemoteControlButton()
        { }

        public RemoteControlButton(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _keyBitFieldMode = SingleValueMode<sbyte, sbyte>(ModeIndexBitField);

            ObserveForPropertyChanged(PlusObservable, nameof(Plus));
            ObserveForPropertyChanged(RedObservable, nameof(Red));
            ObserveForPropertyChanged(MinusObservable, nameof(Minus));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-00-01-02-05-1F-00-00-00
0C-00-44-00-00-00-52-43-4B-45-59-00
0E-00-44-00-00-01-00-00-80-BF-00-00-80-3F
0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-03-00-00-80-BF-00-00-80-3F
0A-00-44-00-00-04-62-74-6E-00
08-00-44-00-00-05-18-00
0A-00-44-00-00-80-01-00-02-00
0C-00-44-00-01-00-4B-45-59-41-20-00
0E-00-44-00-01-01-00-00-80-BF-00-00-80-3F
0E-00-44-00-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-01-03-00-00-80-BF-00-00-80-3F
0A-00-44-00-01-04-62-74-6E-00
08-00-44-00-01-05-70-00
0A-00-44-00-01-80-01-00-02-00
0C-00-44-00-02-00-4B-45-59-52-20-00
0E-00-44-00-02-01-00-00-80-BF-00-00-80-3F
0E-00-44-00-02-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-02-03-00-00-80-BF-00-00-80-3F
0A-00-44-00-02-04-62-74-6E-00
08-00-44-00-02-05-68-00
0A-00-44-00-02-80-01-00-02-00
0C-00-44-00-03-00-4B-45-59-44-20-00
0E-00-44-00-03-01-00-00-00-00-00-00-E0-40
0E-00-44-00-03-02-00-00-00-00-00-00-C8-42
0E-00-44-00-03-03-00-00-00-00-00-00-E0-40
0A-00-44-00-03-04-62-74-6E-00
08-00-44-00-03-05-04-00
0A-00-44-00-03-80-01-00-01-00
0C-00-44-00-04-00-4B-45-59-53-44-00
0E-00-44-00-04-01-00-00-00-00-00-00-80-3F
0E-00-44-00-04-02-00-00-00-00-00-00-C8-42
0E-00-44-00-04-03-00-00-00-00-00-00-80-3F
0A-00-44-00-04-04-62-74-6E-00
08-00-44-00-04-05-04-00
0A-00-44-00-04-80-03-00-01-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));

        protected bool IsBitMaskSet(sbyte bitField, sbyte bitMask)
            => (bitField & bitMask) != 0;
    }
}