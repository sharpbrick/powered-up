namespace SharpBrick.PoweredUp
{
    /// <summary>
    /// Simple tilt orientation state based on 2-axis only
    /// </summary>
    public enum MoveHubTiltSimpleOrientation : sbyte
    {
        Horzontial = 0x00,
        Down = 0x03,
        Left = 0x05,
        Right = 0x07,
        Up = 0x09
    }
}