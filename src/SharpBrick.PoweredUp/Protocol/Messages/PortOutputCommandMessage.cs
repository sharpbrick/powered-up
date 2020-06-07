namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public abstract class PortOutputCommandMessage : PoweredUpMessage
    {
        public PortOutputCommandMessage(PortOutputSubCommand subCommand)
        {
            SubCommand = subCommand;
        }
        public byte PortId { get; set; }
        public PortOutputCommandStartupInformation StartupInformation { get; set; }
        public PortOutputCommandCompletionInformation CompletionInformation { get; set; }

        public PortOutputSubCommand SubCommand { get; set; }
    }

    public class PortOutputCommandWriteDirectMessage : PortOutputCommandMessage
    {
        public PortOutputCommandWriteDirectMessage()
            : base(PortOutputSubCommand.WriteDirect)
        { }
    }
    public class PortOutputCommandWriteDirectModeDataMessage : PortOutputCommandMessage
    {
        public PortOutputCommandWriteDirectModeDataMessage(byte mode)
            : base(PortOutputSubCommand.WriteDirectModeData)
        {
            Mode = mode;
        }

        public byte Mode { get; }
    }

    // spec chapter: 3.27.1
    public class PortOutputCommandStartPowerMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandStartPowerMessage() : base(0x00) { }
        public sbyte Power { get; set; }
    }

    // spec chapter: 3.27.2
    public class PortOutputCommandStartPower2Message : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandStartPower2Message() : base(0x00) { }
        public sbyte Power1 { get; set; }
        public sbyte Power2 { get; set; }
    }

    // spec chapter: 3.27.3
    public class PortOutputCommandSetAccTimeMessage : PortOutputCommandMessage
    {
        public PortOutputCommandSetAccTimeMessage() : base(PortOutputSubCommand.SetAccTime) { }
        public ushort Time { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.4
    public class PortOutputCommandSetDecTimeMessage : PortOutputCommandMessage
    {
        public PortOutputCommandSetDecTimeMessage() : base(PortOutputSubCommand.SetDecTime) { }
        public ushort Time { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.5
    public class PortOutputCommandStartSpeedMessage : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeedMessage() : base(PortOutputSubCommand.StartSpeed) { }
        public sbyte Speed { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.6
    public class PortOutputCommandStartSpeed2Message : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeed2Message() : base(PortOutputSubCommand.StartSpeed2) { }
        public sbyte Speed1 { get; set; }
        public sbyte Speed2 { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.7
    public class PortOutputCommandStartSpeedForTimeMessage : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeedForTimeMessage() : base(PortOutputSubCommand.StartSpeedForTime) { }
        public ushort Time { get; set; }
        public sbyte Speed { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.8
    public class PortOutputCommandStartSpeedForTime2Message : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeedForTime2Message() : base(PortOutputSubCommand.StartSpeedForTime2) { }
        public ushort Time { get; set; }
        public sbyte Speed1 { get; set; }
        public sbyte Speed2 { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.9
    public class PortOutputCommandStartSpeedForDegreesMessage : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeedForDegreesMessage() : base(PortOutputSubCommand.StartSpeedForDegrees) { }
        public uint Degrees { get; set; }
        public sbyte Speed { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.10
    public class PortOutputCommandStartSpeedForDegrees2Message : PortOutputCommandMessage
    {
        public PortOutputCommandStartSpeedForDegrees2Message() : base(PortOutputSubCommand.StartSpeedForDegrees2) { }
        public uint Degrees { get; set; }
        public sbyte Speed1 { get; set; }
        public sbyte Speed2 { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.11
    public class PortOutputCommandGotoAbsolutePositionMessage : PortOutputCommandMessage
    {
        public PortOutputCommandGotoAbsolutePositionMessage() : base(PortOutputSubCommand.GotoAbsolutePosition) { }
        public int AbsolutePosition { get; set; } // UNSPECED: relative to what? (to the position of the motor start)
        public sbyte Speed { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }
    // spec chapter: 3.27.12
    public class PortOutputCommandGotoAbsolutePosition2Message : PortOutputCommandMessage
    {
        public PortOutputCommandGotoAbsolutePosition2Message() : base(PortOutputSubCommand.GotoAbsolutePosition2) { }
        public int AbsPos1 { get; set; }
        public int AbsPos2 { get; set; }
        public sbyte Speed { get; set; }
        public byte MaxPower { get; set; }
        public PortOutputCommandSpecialSpeed EndState { get; set; }
        public PortOutputCommandSpeedProfile Profile { get; set; }
    }

    // spec chapter: 3.27.13
    public class PortOutputCommandPresetEncoderMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandPresetEncoderMessage() : base(0xFF) { }//TODO find out mode
        public int Position { get; set; }
    }

    // spec chapter: 3.27.14
    public class PortOutputCommandPreset2EncoderMessage : PortOutputCommandMessage
    {
        public PortOutputCommandPreset2EncoderMessage() : base(PortOutputSubCommand.PresetEncoder2) { }
        public int Position1 { get; set; }
        public int Position2 { get; set; }
    }

    // spec chapter: 3.27.15
    public class PortOutputCommandTiltImpactPresetMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandTiltImpactPresetMessage() : base(0x03) { }
        public int PresetValue { get; set; }
    }

    // spec chapter: 3.27.16
    public class PortOutputCommandTiltConfigOrientationMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandTiltConfigOrientationMessage() : base(0x05) { }
        public PortOutputCommandTiltConfigOrientation Orientation { get; set; }
    }

    // spec chapter: 3.27.17
    public class PortOutputCommandTiltConfigImpactMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandTiltConfigImpactMessage() : base(0x06) { }
        public sbyte ImpactThreshold { get; set; }
        public sbyte BumpHoldoff { get; set; }
    }

    // spec chapter: 3.27.19
    public class PortOutputCommandGenericZeroSetHardwareMessage : PortOutputCommandWriteDirectMessage
    { }

    // spec chapter: 3.27.20
    public class PortOutputCommandSetRgbColorNoMessage : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandSetRgbColorNoMessage() : base(0x00) { }

        public PortOutputCommandColors ColorNo { get; set; }
    }

    // spec chapter: 3.27.21
    public class PortOutputCommandSetRgbColorNo2Message : PortOutputCommandWriteDirectModeDataMessage
    {
        public PortOutputCommandSetRgbColorNo2Message() : base(0x01) { }

        public byte RedColor { get; set; }
        public byte GreenColor { get; set; }
        public byte BlueColor { get; set; }
    }
}