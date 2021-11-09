using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp;

public class SystemTrainMotor : BasicMotor, IPoweredUpDevice
{
    public SystemTrainMotor()
        : base()
    { }
    public SystemTrainMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
        : base(protocol, hubId, portId)
    { }

    public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
        => @"
0B-00-43-00-01-01-01-00-00-01-00
05-00-43-00-02
12-00-44-00-00-00-4C-50-46-32-2D-54-52-41-49-4E-00-00
0E-00-44-00-00-01-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-00-00-03-00-00-C8-C2-00-00-C8-42
0B-00-44-00-00-04-00-00-00-00-00
08-00-44-00-00-05-00-18
0A-00-44-00-00-80-01-00-04-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
}
