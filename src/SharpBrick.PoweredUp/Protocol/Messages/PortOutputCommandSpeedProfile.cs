using System;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    [Flags]
    public enum PortOutputCommandSpeedProfile : byte
    {
        None = 0x00,
        AccelerationProfile = 0x01,
        DeccelerationProfile = 0x02,
    }
}