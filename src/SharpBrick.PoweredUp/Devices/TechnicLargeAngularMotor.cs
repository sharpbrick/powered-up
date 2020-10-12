using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class TechnicLargeAngularMotor : AbsoluteMotor, IPoweredUpDevice
    {
        public TechnicLargeAngularMotor()
            : base()
        { }
        public TechnicLargeAngularMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => @"
0B-00-43-00-01-0F-06-0E-00-0F-00
07-00-43-00-02-0E-00
11-00-44-00-00-00-50-4F-57-45-52-00-00-00-00-00-00
0E-00-44-00-00-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-03-00-00-C8-C2-00-00-C8-42
0A-00-44-00-00-04-50-43-54-00
08-00-44-00-00-05-00-50
0A-00-44-00-00-80-01-00-04-00
11-00-44-00-01-00-53-50-45-45-44-00-00-00-00-00-00
0E-00-44-00-01-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-01-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-01-03-00-00-C8-C2-00-00-C8-42
0A-00-44-00-01-04-50-43-54-00
08-00-44-00-01-05-30-70
0A-00-44-00-01-80-01-00-04-00
11-00-44-00-02-00-50-4F-53-00-00-00-00-00-00-00-00
0E-00-44-00-02-01-00-00-B4-C3-00-00-B4-43
0E-00-44-00-02-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-02-03-00-00-B4-C3-00-00-B4-43
0A-00-44-00-02-04-44-45-47-00
08-00-44-00-02-05-28-68
0A-00-44-00-02-80-01-02-0B-00
11-00-44-00-03-00-41-50-4F-53-00-00-00-00-00-00-00
0E-00-44-00-03-01-00-00-34-C3-00-00-33-43
0E-00-44-00-03-02-00-00-48-C3-00-00-48-43
0E-00-44-00-03-03-00-00-34-C3-00-00-33-43
0A-00-44-00-03-04-44-45-47-00
08-00-44-00-03-05-32-72
0A-00-44-00-03-80-01-01-03-00
11-00-44-00-04-00-43-41-4C-49-42-00-00-00-00-00-00
0E-00-44-00-04-01-00-00-00-00-00-00-61-45
0E-00-44-00-04-02-00-00-00-00-00-00-C8-42
0E-00-44-00-04-03-00-00-00-00-00-00-61-45
0A-00-44-00-04-04-43-41-4C-00
08-00-44-00-04-05-00-00
0A-00-44-00-04-80-02-01-05-00
11-00-44-00-05-00-53-54-41-54-53-00-00-00-00-00-00
0E-00-44-00-05-01-00-00-00-00-00-FF-7F-47
0E-00-44-00-05-02-00-00-00-00-00-00-C8-42
0E-00-44-00-05-03-00-00-00-00-00-FF-7F-47
0A-00-44-00-05-04-4D-49-4E-00
08-00-44-00-05-05-00-00
0A-00-44-00-05-80-0E-01-05-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}