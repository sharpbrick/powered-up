using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Knowledge;

namespace SharpBrick.PoweredUp.Devices
{
    public interface IPowerdUpDevice
    {
        IEnumerable<byte[]> GetStaticPortInfoMessages(Version sw, Version hw);
    }
}