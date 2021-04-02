using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace SharpBrick.PoweredUp.Devices
{
    public class RemoteControlRssi : Device, IPoweredUpDevice
    {
        protected SingleValueMode<sbyte, sbyte> _rssiMode;
        public byte ModeIndexRssi { get; protected set; } = 0;

        public sbyte Rssi => _rssiMode.SI;
        public IObservable<sbyte> RssiObservable => _rssiMode.Observable.Select(x => x.SI);

        public RemoteControlRssi()
        { }

        public RemoteControlRssi(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
            _rssiMode = SingleValueMode<sbyte, sbyte>(ModeIndexRssi);

            ObserveForPropertyChanged(_rssiMode.Observable, nameof(Rssi));
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-3C-01-02-01-01-00-00-00
0C-00-44-3C-00-00-52-53-53-49-20-00
0E-00-44-3C-00-01-00-00-A0-C2-00-00-F0-C1
0E-00-44-3C-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3C-00-03-00-00-A0-C2-00-00-F0-C1
0A-00-44-3C-00-04-64-62-6D-00
08-00-44-3C-00-05-50-00
0A-00-44-3C-00-80-01-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}
