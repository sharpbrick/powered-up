namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.4
    public enum HubAttachedIOType
    {
        Motor = 0x0001,
        SystemTrainMotor = 0x0002,
        Button = 0x0005,
        LedLight = 0x0008,
        Voltage = 0x0014,
        Current = 0x0015,
        PiezoToneSound = 0x0016,
        RgbLight = 0x0017,
        ExternalTiltSensor = 0x0022,
        MotionSensor = 0x0023,
        VisionSensor = 0x0025,
        ExternalMotorWithTacho = 0x0026,
        InternalMotorWithTacho = 0x0027,
        InternalTilt = 0x0028,
    }
}