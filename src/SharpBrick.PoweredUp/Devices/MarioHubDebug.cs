using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MarioHubDebug : Device, IPoweredUpDevice
    {
        public MarioHubDebug()
        { }

        public MarioHubDebug(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        {
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-03-01-02-04-0F-00-00-00
05-00-43-03-02
11-00-44-03-00-00-43-48-41-4C-00-00-00-00-00-00-00
0E-00-44-03-00-01-00-00-00-00-00-FF-7F-47
0E-00-44-03-00-02-00-00-00-00-00-00-C8-42
0E-00-44-03-00-03-00-00-00-00-00-FF-7F-47
0A-00-44-03-00-04-6E-61-00-00
08-00-44-03-00-05-84-00
0A-00-44-03-00-80-02-01-03-00
11-00-44-03-01-00-56-45-52-53-00-00-00-00-00-00-00
0E-00-44-03-01-01-00-00-00-00-00-00-7F-43
0E-00-44-03-01-02-00-00-00-00-00-00-C8-42
0E-00-44-03-01-03-00-00-00-00-00-00-7F-43
0A-00-44-03-01-04-6E-61-00-00
08-00-44-03-01-05-84-00
0A-00-44-03-01-80-04-02-0A-00
11-00-44-03-02-00-45-56-45-4E-54-53-00-00-00-00-00
0E-00-44-03-02-01-00-00-00-00-00-FF-7F-47
0E-00-44-03-02-02-00-00-00-00-00-00-C8-42
0E-00-44-03-02-03-00-00-00-00-00-FF-7F-47
0A-00-44-03-02-04-6E-61-00-00
08-00-44-03-02-05-84-00
0A-00-44-03-02-80-02-01-0A-00
11-00-44-03-03-00-44-45-42-55-47-00-00-00-00-00-00
0E-00-44-03-03-01-00-00-00-00-00-FF-7F-47
0E-00-44-03-03-02-00-00-00-00-00-00-C8-42
0E-00-44-03-03-03-00-00-00-00-00-FF-7F-47
0A-00-44-03-03-04-6E-61-00-00
08-00-44-03-03-05-84-00
0A-00-44-03-03-80-04-02-0A-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}