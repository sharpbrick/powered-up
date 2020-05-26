namespace SharpBrick.PoweredUp.Protocol
{
    // spec chapter: 3.7.2
    public enum HubAlert : byte
    {
        LowVoltage = 0x01,
        HighCurrent = 0x02,
        LowSignalStrength = 0x03,
        OverPowerCondition = 0x04,
    }
}