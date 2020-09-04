namespace SharpBrick.PoweredUp
{
    // spec chapter: 3.8.4
    public enum DeviceType : ushort
    {
        Unknown = 0x000, // UNSPECED
        Motor = 0x0001, // SIMPLE_MEDIUM_LINEAR_MOTOR
        SystemTrainMotor = 0x0002, // TRAIN_MOTOR
        Button = 0x0005, // not mentioned in node-poweredup
        LedLight = 0x0008, // LIGHT
        Voltage = 0x0014, // VOLTAGE_SENSOR
        Current = 0x0015, // CURRENT_SENSOR
        PiezoToneSound = 0x0016, // PIEZO_BUZZER
        RgbLight = 0x0017, // HUB_LED
        ExternalTiltSensor = 0x0022, // TILT_SENSOR
        MotionSensor = 0x0023, // MOTION_SENSOR
        VisionSensor = 0x0025, // COLOR_DISTANCE_SENSOR
        ExternalMotorWithTacho = 0x0026, // MEDIUM_LINEAR_MOTOR
        InternalMotorWithTacho = 0x0027, // MOVE_HUB_MEDIUM_LINEAR_MOTOR
        InternalTilt = 0x0028, // MOVE_HUB_TILT_SENSOR

        DuploTrainBaseMotor = 0x0029, // UNSPECED, DUPLO_TRAIN_BASE_MOTOR
        DuploTrainBaseSpeaker = 0x002A, // UNSPECED, DUPLO_TRAIN_BASE_SPEAKER
        DuploTrainBaseColorSensor = 0x002B, // UNSPECED, DUPLO_TRAIN_BASE_COLOR_SENSOR
        DuploTrainBaseSpeedometer = 0x002C, // UNSPECED, DUPLO_TRAIN_BASE_SPEEDOMETER
        TechnicLargeLinearMotor = 0x002E, // UNSPECED, TECHNIC_LARGE_LINEAR_MOTOR
        TechnicXLargeLinearMotor = 0x002F, // UNSPECED, TECHNIC_XLARGE_LINEAR_MOTOR
        TechnicMediumAngularMotor = 0x0030, // UNSPECED, TECHNIC_MEDIUM_ANGULAR_MOTOR (Spike Prime)
        TechnicLargeAngularMotor = 0x0031, // UNSPECED, TECHNIC_LARGE_ANGULAR_MOTOR (Spike Prime)
        TechnicMediumHubGestureSensor = 0x0036, // UNSPECED, TECHNIC_MEDIUM_HUB_GEST_SENSOR
        RemoteControlButton = 0x0037, // UNSPECED, REMOTE_CONTROL_BUTTON
        RemoteControlRssi = 0x0038, // UNSPECED, REMOTE_CONTROL_RSSI
        TechnicMediumHubAccelerometer = 0x0039, // UNSPECED, TECHNIC_MEDIUM_HUB_ACCELEROMETER
        TechnicMediumHubGyroSensor = 0x003A, // UNSPECED, TECHNIC_MEDIUM_HUB_GYRO_SENSOR
        TechnicMediumHubTiltSensor = 0x003B, // UNSPECED, TECHNIC_MEDIUM_HUB_TILT_SENSOR
        TechnicMediumHubTemperatureSensor = 0x003C, // UNSPECED, TECHNIC_MEDIUM_HUB_TEMPERATURE_SENSOR
        TechnicColorSensor = 0x003D, // UNSPECED, TECHNIC_COLOR_SENSOR (Spike Prime)
        TechnicDistanceSensor = 0x003E, // UNSPECED, TECHNIC_DISTANCE_SENSOR (Spike Prime)
        TechnicForceSensor = 0x003F, // UNSPECED, TECHNIC_FORCE_SENSOR (Spike Prime)
    }
}