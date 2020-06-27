using System;

namespace SharpBrick.PoweredUp
{
    [Flags]
    public enum PortFeedback : byte
    {
        BufferEmptyAndCommandInProcess = 0x01,
        BufferEmptyAndCommandCompleted = 0x02,
        CurrentCommandsDiscarded = 0x04,
        Idle = 0x08,
        BusyFull = 0x10,
    }
}