using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Devices
{
    public class TechnicMediumHubTiltSensor : IPowerdUpDevice
    {
        public void ApplyStaticPortInfo(PortInfo port)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version sw, Version hw)
            =>
@"
0B-00-43-63-01-03-03-03-00-04-00
05-00-43-63-02
11-00-44-63-00-00-50-4F-53-00-00-00-00-00-00-00-00
0E-00-44-63-00-01-00-00-34-C3-00-00-34-43
0E-00-44-63-00-02-00-00-C8-C2-00-00-C8-42
0E-00-44-63-00-03-00-00-34-C3-00-00-34-43
0A-00-44-63-00-04-44-45-47-00
08-00-44-63-00-05-50-00
0A-00-44-63-00-80-03-01-03-00
11-00-44-63-01-00-49-4D-50-00-00-00-00-00-00-00-00
0E-00-44-63-01-01-00-00-00-00-00-00-C8-42
0E-00-44-63-01-02-00-00-00-00-00-00-C8-42
0E-00-44-63-01-03-00-00-00-00-00-00-C8-42
0A-00-44-63-01-04-43-4E-54-00
08-00-44-63-01-05-08-00
0A-00-44-63-01-80-01-02-03-00
11-00-44-63-02-00-43-46-47-00-00-00-00-00-00-00-00
0E-00-44-63-02-01-00-00-00-00-00-00-7F-43
0E-00-44-63-02-02-00-00-00-00-00-00-C8-42
0E-00-44-63-02-03-00-00-00-00-00-00-7F-43
0A-00-44-63-02-04-00-00-00-00
08-00-44-63-02-05-00-10
0A-00-44-63-02-80-02-00-03-00
".Trim().Split("\n").Select(s => BytesStringUtil.StringToData(s));
    }
}