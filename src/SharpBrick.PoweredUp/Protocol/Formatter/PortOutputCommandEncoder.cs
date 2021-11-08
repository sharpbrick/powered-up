using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortOutputCommandEncoder : IMessageContentEncoder
{
    public ushort CalculateContentLength(LegoWirelessMessage message)
        => (ushort)(message switch
        {
            PortOutputCommandMessage portOutputMessage => 3 + portOutputMessage switch
            {
                PortOutputCommandStartPower2Message => 2,
                PortOutputCommandSetAccTimeMessage => 3,
                PortOutputCommandSetDecTimeMessage => 3,
                PortOutputCommandStartSpeedMessage => 3,
                PortOutputCommandStartSpeed2Message => 4,
                PortOutputCommandStartSpeedForTimeMessage => 6,
                PortOutputCommandStartSpeedForTime2Message => 7,
                PortOutputCommandStartSpeedForDegreesMessage => 8,
                PortOutputCommandStartSpeedForDegrees2Message => 9,
                PortOutputCommandGotoAbsolutePositionMessage => 8,
                PortOutputCommandGotoAbsolutePosition2Message => 12,
                PortOutputCommandWriteDirectModeDataMessage directWriteModeDataMessage => 1 + directWriteModeDataMessage switch
                {
                    GenericWriteDirectModeDataMessage msg => msg.Data.Length,
                    PortOutputCommandStartPowerMessage => 1,
                    PortOutputCommandSetRgbColorNoMessage => 1,
                    PortOutputCommandSetRgbColorNo2Message => 3,
                    PortOutputCommandPresetEncoderMessage => 4,
                    PortOutputCommandTiltImpactPresetMessage => 4,
                    PortOutputCommandTiltConfigImpactMessage => 2,
                    PortOutputCommandTiltConfigOrientationMessage => 1,

                    _ => throw new NotSupportedException(),
                },

                _ => throw new NotSupportedException(),
            },

            _ => throw new NotSupportedException(),
        });

    public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        => throw new NotImplementedException();

    public void Encode(LegoWirelessMessage message, in Span<byte> data)
        => Encode(message as PortOutputCommandMessage ?? throw new InvalidOperationException(), data);

    public void Encode(PortOutputCommandMessage message, in Span<byte> data)
    {
        data[0] = message.PortId;
        data[1] = (byte)(((byte)message.StartupInformation) << 4 | ((byte)message.CompletionInformation));
        data[2] = (byte)message.SubCommand;

        switch (message)
        {
            case PortOutputCommandStartPower2Message msg:
                data[3] = (byte)msg.Power1;
                data[4] = (byte)msg.Power2;
                break;

            case PortOutputCommandSetAccTimeMessage msg:
                BitConverter.TryWriteBytes(data.Slice(3, 2), msg.Time);
                data[5] = (byte)msg.Profile;
                break;
            case PortOutputCommandSetDecTimeMessage msg:
                BitConverter.TryWriteBytes(data.Slice(3, 2), msg.Time);
                data[5] = (byte)msg.Profile;
                break;
            case PortOutputCommandStartSpeedMessage msg:
                data[3] = (byte)msg.Speed;
                data[4] = msg.MaxPower;
                data[5] = (byte)msg.Profile;
                break;
            case PortOutputCommandStartSpeed2Message msg:
                data[3] = (byte)msg.Speed1;
                data[4] = (byte)msg.Speed2;
                data[5] = msg.MaxPower;
                data[6] = (byte)msg.Profile;
                break;

            case PortOutputCommandStartSpeedForTimeMessage msg:
                BitConverter.TryWriteBytes(data.Slice(3, 2), msg.Time);
                data[5] = (byte)msg.Speed;
                data[6] = msg.MaxPower;
                data[7] = (byte)msg.EndState;
                data[8] = (byte)msg.Profile;
                break;
            case PortOutputCommandStartSpeedForTime2Message msg:
                BitConverter.TryWriteBytes(data.Slice(3, 2), msg.Time);
                data[5] = (byte)msg.Speed1;
                data[6] = (byte)msg.Speed2;
                data[7] = msg.MaxPower;
                data[8] = (byte)msg.EndState;
                data[9] = (byte)msg.Profile;
                break;

            case PortOutputCommandStartSpeedForDegreesMessage msg:
                BitConverter.TryWriteBytes(data.Slice(3, 4), msg.Degrees);
                data[7] = (byte)msg.Speed;
                data[8] = msg.MaxPower;
                data[9] = (byte)msg.EndState;
                data[10] = (byte)msg.Profile;
                break;

            case PortOutputCommandStartSpeedForDegrees2Message msg:
                BitConverter.TryWriteBytes(data.Slice(3, 4), msg.Degrees);
                data[7] = (byte)msg.Speed1;
                data[8] = (byte)msg.Speed2;
                data[9] = msg.MaxPower;
                data[10] = (byte)msg.EndState;
                data[11] = (byte)msg.Profile;
                break;

            case PortOutputCommandGotoAbsolutePositionMessage msg:
                BitConverter.TryWriteBytes(data.Slice(3, 4), msg.AbsolutePosition);
                data[7] = (byte)msg.Speed;
                data[8] = msg.MaxPower;
                data[9] = (byte)msg.EndState;
                data[10] = (byte)msg.Profile;
                break;

            case PortOutputCommandGotoAbsolutePosition2Message msg:
                BitConverter.TryWriteBytes(data.Slice(3, 4), msg.AbsolutePosition1);
                BitConverter.TryWriteBytes(data.Slice(7, 4), msg.AbsolutePosition2);
                data[11] = (byte)msg.Speed;
                data[12] = msg.MaxPower;
                data[13] = (byte)msg.EndState;
                data[14] = (byte)msg.Profile;
                break;

            case PortOutputCommandWriteDirectModeDataMessage directWriteModeDataMessage:
                data[3] = directWriteModeDataMessage.ModeIndex;

                switch (directWriteModeDataMessage)
                {
                    case GenericWriteDirectModeDataMessage msg:
                        msg.Data.CopyTo(data[4..]);
                        break;
                    case PortOutputCommandStartPowerMessage msg:
                        data[4] = (byte)msg.Power;
                        break;

                    case PortOutputCommandSetRgbColorNoMessage msg:
                        data[4] = (byte)msg.ColorNo;
                        break;

                    case PortOutputCommandSetRgbColorNo2Message msg:
                        data[4] = msg.RedColor;
                        data[5] = msg.GreenColor;
                        data[6] = msg.BlueColor;
                        break;

                    case PortOutputCommandPresetEncoderMessage msg:
                        BitConverter.TryWriteBytes(data.Slice(4, 4), msg.Position);
                        break;

                    case PortOutputCommandTiltImpactPresetMessage msg:
                        BitConverter.TryWriteBytes(data.Slice(4, 4), msg.PresetValue);
                        break;

                    case PortOutputCommandTiltConfigImpactMessage msg:
                        data[4] = (byte)msg.ImpactThreshold;
                        data[5] = (byte)msg.BumpHoldoff;
                        break;

                    case PortOutputCommandTiltConfigOrientationMessage msg:
                        data[4] = (byte)msg.Orientation;
                        break;
                }

                break;
        }
    }
}
