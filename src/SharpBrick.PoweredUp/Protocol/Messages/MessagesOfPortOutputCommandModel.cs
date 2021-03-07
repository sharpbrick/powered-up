using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortOutputCommandFeedbackMessage(PortOutputCommandFeedback[] Feedbacks)
        : LegoWirelessMessage(MessageType.PortOutputCommandFeedback)
    {
        public override string ToString()
            => $"Port Output Command Feedback - " + string.Join(",", this.Feedbacks.Select(f => $"Port {this.HubId}/{f.PortId} -> {f.Feedback}"));
    }

    public abstract record PortOutputCommandMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            PortOutputSubCommand SubCommand
        ) : LegoWirelessMessage(MessageType.PortOutputCommand);

    public record PortOutputCommandWriteDirectMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.WriteDirect);

    public record PortOutputCommandWriteDirectModeDataMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            byte ModeIndex
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.WriteDirectModeData);

    public record GenericWriteDirectModeDataMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            byte ModeIndex,
            byte[] Data
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, ModeIndex);

    // spec chapter: 3.27.1
    public record PortOutputCommandStartPowerMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            sbyte Power
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x00);

    // spec chapter: 3.27.2
    public record PortOutputCommandStartPower2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            sbyte Power1,
            sbyte Power2
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartPower2);

    // spec chapter: 3.27.3
    public record PortOutputCommandSetAccTimeMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            ushort Time,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.SetAccTime);

    // spec chapter: 3.27.4
    public record PortOutputCommandSetDecTimeMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            ushort Time,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.SetDecTime);

    // spec chapter: 3.27.5
    public record PortOutputCommandStartSpeedMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            sbyte Speed,
            byte MaxPower,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeed);

    // spec chapter: 3.27.6
    public record PortOutputCommandStartSpeed2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            sbyte Speed1,
            sbyte Speed2,
            byte MaxPower,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeed2);

    // spec chapter: 3.27.7
    public record PortOutputCommandStartSpeedForTimeMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            ushort Time,
            sbyte Speed,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeedForTime);

    // spec chapter: 3.27.8
    public record PortOutputCommandStartSpeedForTime2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            ushort Time,
            sbyte Speed1,
            sbyte Speed2,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeedForTime2);

    // spec chapter: 3.27.9
    public record PortOutputCommandStartSpeedForDegreesMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            uint Degrees,
            sbyte Speed,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeedForDegrees);

    // spec chapter: 3.27.10
    public record PortOutputCommandStartSpeedForDegrees2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            uint Degrees,
            sbyte Speed1,
            sbyte Speed2,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.StartSpeedForDegrees2);

    // spec chapter: 3.27.11
    public record PortOutputCommandGotoAbsolutePositionMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            int AbsolutePosition,
            sbyte Speed,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.GotoAbsolutePosition);
    // spec chapter: 3.27.12
    public record PortOutputCommandGotoAbsolutePosition2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            int AbsolutePosition1,
            int AbsolutePosition2,
            sbyte Speed,
            byte MaxPower,
            SpecialSpeed EndState,
            SpeedProfiles Profile
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.GotoAbsolutePosition2);

    // spec chapter: 3.27.13
    public record PortOutputCommandPresetEncoderMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            int Position
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0xFF); //TODO find out mode

    // spec chapter: 3.27.14
    public record PortOutputCommandPreset2EncoderMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            int Position1,
            int Position2
        ) : PortOutputCommandMessage(PortId, StartupInformation, CompletionInformation, PortOutputSubCommand.PresetEncoder2);

    // spec chapter: 3.27.15
    public record PortOutputCommandTiltImpactPresetMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            int PresetValue
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x03);

    // spec chapter: 3.27.16
    public record PortOutputCommandTiltConfigOrientationMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            TiltConfigOrientation Orientation
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x05);

    // spec chapter: 3.27.17
    public record PortOutputCommandTiltConfigImpactMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            sbyte ImpactThreshold,
            sbyte BumpHoldoff
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x06);

    // spec chapter: 3.27.19
    public record PortOutputCommandGenericZeroSetHardwareMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation
        ) : PortOutputCommandWriteDirectMessage(PortId, StartupInformation, CompletionInformation);

    // spec chapter: 3.27.20
    public record PortOutputCommandSetRgbColorNoMessage(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            PoweredUpColor ColorNo
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x00);

    // spec chapter: 3.27.21
    public record PortOutputCommandSetRgbColorNo2Message(
            byte PortId,
            PortOutputCommandStartupInformation StartupInformation,
            PortOutputCommandCompletionInformation CompletionInformation,
            byte RedColor,
            byte GreenColor,
            byte BlueColor
        ) : PortOutputCommandWriteDirectModeDataMessage(PortId, StartupInformation, CompletionInformation, 0x01);
}