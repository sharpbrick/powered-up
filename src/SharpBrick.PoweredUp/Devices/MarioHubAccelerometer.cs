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
0B-00-43-00-01-06-02-03-00-00-00
07-00-43-00-02-03-00
11-00-44-00-00-00-52-41-57-00-00-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-00-00-00-00-C8-42
0E-00-44-00-00-02-00-00-00-00-00-00-C8-42
0E-00-44-00-00-03-00-00-00-00-00-00-C8-42
0A-00-44-00-00-04-63-6E-74-00
08-00-44-00-00-05-84-00
0A-00-44-00-00-80-03-00-03-00
11-00-44-00-01-00-47-45-53-54-00-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-00-00-00-00-C8-42
0E-00-44-00-01-02-00-00-00-00-00-00-C8-42
0E-00-44-00-01-03-00-00-00-00-00-00-C8-42
0A-00-44-00-01-04-63-6E-74-00
08-00-44-00-01-05-84-00
0A-00-44-00-01-80-02-01-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}