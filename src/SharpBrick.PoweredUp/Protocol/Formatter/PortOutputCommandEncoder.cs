using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortOutputCommandEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => (ushort)(message switch
            {
                PortOutputCommandMessage portOutputMessage => 3 + portOutputMessage switch
                {
                    PortOutputCommandStartPower2Message msg => 2,
                    PortOutputCommandSetAccTimeMessage msg => 3,
                    PortOutputCommandSetDecTimeMessage msg => 3,
                    PortOutputCommandStartSpeedMessage msg => 3,
                    PortOutputCommandStartSpeed2Message msg => 4,
                    PortOutputCommandStartSpeedForTimeMessage msg => 6,
                    PortOutputCommandStartSpeedForTime2Message msg => 7,
                    PortOutputCommandStartSpeedForDegreesMessage msg => 8,
                    PortOutputCommandStartSpeedForDegrees2Message msg => 9,
                    PortOutputCommandGotoAbsolutePositionMessage msg => 8,
                    PortOutputCommandGotoAbsolutePosition2Message msg => 12,
                    PortOutputCommandWriteDirectModeDataMessage directWriteModeDataMessage => 1 + directWriteModeDataMessage switch
                    {
                        PortOutputCommandStartPowerMessage msg => 1,
                        PortOutputCommandSetRgbColorNoMessage msg => 1,
                        PortOutputCommandSetRgbColorNo2Message msg => 3,

                        _ => throw new NotSupportedException(),
                    },

                    _ => throw new NotSupportedException(),
                },

                _ => throw new NotSupportedException(),
            });

        public PoweredUpMessage Decode(in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(PoweredUpMessage message, in Span<byte> data)
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
                    data[3] = directWriteModeDataMessage.Mode;

                    switch (directWriteModeDataMessage)
                    {
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
                    }

                    break;
            }
        }
    }
}