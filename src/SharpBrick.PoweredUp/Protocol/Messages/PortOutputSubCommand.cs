namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public enum PortOutputSubCommand : byte
    {
        StartPower2 = 0x02,
        SetAccTime = 0x05,
        SetDecTime = 0x06,
        StartSpeed = 0x07,
        StartSpeed2 = 0x08,
        StartSpeedForTime = 0x09,
        StartSpeedForTime2 = 0x0A,
        StartSpeedForDegrees = 0x0B,
        StartSpeedForDegrees2 = 0x0C,
        GotoAbsolutePosition = 0x0D,
        GotoAbsolutePosition2 = 0x0E,
        PresetEncoder2 = 0x14,
        WriteDirect = 0x50,
        WriteDirectModeData = 0x51,
    }
}