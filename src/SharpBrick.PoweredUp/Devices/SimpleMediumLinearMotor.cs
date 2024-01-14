using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp;

// TODO which motor to extend here. ?
public class SimpleMediumLinearMotor : BasicMotor, IPoweredUpDevice
{
    public SimpleMediumLinearMotor()
        : base()
    { }
    public SimpleMediumLinearMotor(ILegoWirelessProtocol protocol, byte hubId, byte portId)
        : base(protocol, hubId, portId)
    { }

    public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType) 
      => @"
0B-00-43-02-01-01-01-00-00-01-00
05-00-43-02-02
11-00-44-02-00-00-4C-50-46-32-2D-4D-4D-4F-54-4F-52
0E-00-44-02-00-01-00-00-C8-C2-00-00-C8-42
0E-00-44-02-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-02-00-03-00-00-C8-C2-00-00-C8-42
0A-00-44-02-00-04-00-00-00-00
08-00-44-02-00-05-00-10
0A-00-44-02-00-80-01-00-04-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
}
