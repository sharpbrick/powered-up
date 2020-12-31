namespace SharpBrick.PoweredUp
{
    /// <summary>
    /// Simple tilt orientation state based on 3-axis providing more possible states than <see cref="MoveHubTiltSimpleOrientation"/>
    /// </summary>
    public enum MoveHubTiltOrientation : sbyte
    {
        Bottom = 0x00,
        Front = 0x01,
        Back = 0x02,
        Left = 0x03,
        Right = 0x04,
        Top = 0x05,
    }
}