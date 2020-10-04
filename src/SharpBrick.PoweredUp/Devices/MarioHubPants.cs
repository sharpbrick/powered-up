using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubPants : Device, IPoweredUpDevice
    {
        public MarioHubPants()
        { }

        public MarioHubPants(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => @"
0B-00-43-02-01-02-01-01-00-00-00
05-00-43-02-02
11-00-44-02-00-00-50-41-4E-54-00-00-00-00-00-00-00
0E-00-44-02-00-01-00-00-00-00-00-00-7C-42
0E-00-44-02-00-02-00-00-00-00-00-00-C8-42
0E-00-44-02-00-03-00-00-00-00-00-00-7C-42
0A-00-44-02-00-04-69-64-78-00
08-00-44-02-00-05-84-00
0A-00-44-02-00-80-01-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}