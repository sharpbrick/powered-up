using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class MediumLinearMotor : TachoMotor, IPoweredUpDevice
    {
        public MediumLinearMotor()
            : base()
        { }
        public MediumLinearMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-00-01-07-03-06-00-07-00
07-00-43-00-02-06-00
12-00-44-00-00-00-50-4F-57-45-52-00-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-03-00-00-C8-C2-00-00-C8-42
0B-00-44-00-00-04-50-43-54-00-00
08-00-44-00-00-05-00-10
0A-00-44-00-00-80-01-00-01-00
12-00-44-00-01-00-53-50-45-45-44-00-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-01-03-00-00-C8-C2-00-00-C8-42
0B-00-44-00-01-04-50-43-54-00-00
08-00-44-00-01-05-10-10
0A-00-44-00-01-80-01-00-04-00
12-00-44-00-02-00-50-4F-53-00-00-00-00-00-00-00-00-00
0E-00-44-00-02-01-00-00-B4-C3-00-00-B4-43
0E-00-44-00-02-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-02-03-00-00-B4-C3-00-00-B4-43
0B-00-44-00-02-04-44-45-47-00-00
08-00-44-00-02-05-08-08
0A-00-44-00-02-80-01-02-04-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}