using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Protocol.Knowledge;

namespace SharpBrick.PoweredUp;

public interface IPoweredUpDevice
{
    bool IsConnected { get; }
    IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType);
    void ExtendPortMode(PortModeInfo portModeInfo)
    { }
}
