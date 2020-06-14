using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicMediumHubAccelerometer : Device, IPoweredUpDevice
    {
        public TechnicMediumHubAccelerometer()
        { }

        public TechnicMediumHubAccelerometer(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
0B-00-43-61-01-02-02-03-00-00-00
05-00-43-61-02
11-00-44-61-00-00-47-52-56-00-00-00-00-00-00-00-00
0E-00-44-61-00-01-00-00-00-C7-00-00-00-47
0E-00-44-61-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-61-00-03-00-00-FA-C5-00-00-FA-45
0A-00-44-61-00-04-6D-47-00-00
08-00-44-61-00-05-50-00
0A-00-44-61-00-80-03-01-03-00
11-00-44-61-01-00-43-41-4C-00-00-00-00-00-00-00-00
0E-00-44-61-01-01-00-00-80-3F-00-00-80-3F
0E-00-44-61-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-61-01-03-00-00-80-3F-00-00-80-3F
0A-00-44-61-01-04-00-00-00-00
08-00-44-61-01-05-50-00
0A-00-44-61-01-80-01-00-00-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}