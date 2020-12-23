using System;

namespace SharpBrick.PoweredUp
{
    [Flags]
    public enum MarioGestures : ushort
    {
        None = 0b0000_0000_0000_0000,
        Bump = 0b0000_0000_0000_0001,
        Gesture2 = 0b0000_0000_0000_0010,
        Gesture4 = 0b0000_0000_0000_0100,
        Shake = 0b0000_0000_0001_0000,
        Gesture64 = 0b0000_0000_0100_0000,
        Gesture128 = 0b0000_0000_1000_0000,
        Turning = 0b0000_0001_0000_0000,
        FastMove = 0b0000_0010_0000_0000,
        Translation = 0b0000_0100_0000_0000,
        HighFallCrash = 0b0000_1000_0000_0000,
        DirectionChange = 0b0001_0000_0000_0000,
        Reverse = 0b0010_0000_0000_0000,
        Gesture16384 = 0b0100_0000_0000_0000,
        Jump = 0b1000_0000_0000_0000,
    }
}