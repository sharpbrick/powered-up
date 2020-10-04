using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubTagSensor : Device, IPoweredUpDevice
    {
        public MarioHubTagSensor()
        { }

        public MarioHubTagSensor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
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