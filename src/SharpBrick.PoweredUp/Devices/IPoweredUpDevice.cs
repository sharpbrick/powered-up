using System;
using System.Collections.Generic;

namespace SharpBrick.PoweredUp
{
    public interface IPoweredUpDevice
    {
        bool IsConnected { get; }
        IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType);
    }
}