using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubAccelerometer : Device, IPoweredUpDevice
    {
        public MarioHubAccelerometer()
        { }

        public MarioHubAccelerometer(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}