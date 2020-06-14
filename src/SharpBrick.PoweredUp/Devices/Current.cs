using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class Current : Device, IPoweredUpDevice
    {
        public Current()
        { }

        public Current(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
0B-00-43-3B-01-02-02-03-00-00-00
05-00-43-3B-02
11-00-44-3B-00-00-43-55-52-20-4C-00-00-00-00-00-00
0E-00-44-3B-00-01-00-00-00-00-00-F0-7F-45
0E-00-44-3B-00-02-00-00-00-00-00-00-C8-42
0E-00-44-3B-00-03-00-00-00-00-00-78-82-45
0A-00-44-3B-00-04-6D-41-00-00
08-00-44-3B-00-05-10-00
0A-00-44-3B-00-80-01-01-04-00
11-00-44-3B-01-00-43-55-52-20-53-00-00-00-00-00-00
0E-00-44-3B-01-01-00-00-00-00-00-F0-7F-45
0E-00-44-3B-01-02-00-00-00-00-00-00-C8-42
0E-00-44-3B-01-03-00-00-00-00-00-78-82-45
0A-00-44-3B-01-04-6D-41-00-00
08-00-44-3B-01-05-10-00
0A-00-44-3B-01-80-01-01-04-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}