using System;

namespace SharpBrick.PoweredUp
{
    // UNSPECED concept of profileNumber etc
    [Flags]
    public enum SpeedProfiles : byte
    {
        None = 0x00,
        AccelerationProfile = 0x01,
        DecelerationProfile = 0x02,
    }
}