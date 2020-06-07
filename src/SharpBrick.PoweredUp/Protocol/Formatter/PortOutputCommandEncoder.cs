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
                    PortOutputCommandSetAccTimeMessage msg => 3,
                    PortOutputCommandSetDecTimeMessage msg => 3,
                    PortOutputCommandStartSpeedMessage msg => 3,
                    PortOutputCommandStartSpeedForTimeMessage msg => 6,
                    PortOutputCommandStartSpeedForDegreesMessage msg => 8,
                    PortOutputCommandGotoAbsolutePositionMessage msg => 8,
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

                case PortOutputCommandStartSpeedForTimeMessage msg:
                    BitConverter.TryWriteBytes(data.Slice(3, 2), msg.Time);
                    data[5] = (byte)msg.Speed;
                    data[6] = msg.MaxPower;
                    data[7] = (byte)msg.EndState;
                    data[8] = (byte)msg.Profile;
                    break;

                case PortOutputCommandStartSpeedForDegreesMessage msg:
                    BitConverter.TryWriteBytes(data.Slice(3, 4), msg.Degrees);
                    data[7] = (byte)msg.Speed;
                    data[8] = msg.MaxPower;
                    data[9] = (byte)msg.EndState;
                    data[10] = (byte)msg.Profile;
                    break;

                case PortOutputCommandGotoAbsolutePositionMessage msg:
                    BitConverter.TryWriteBytes(data.Slice(3, 4), msg.AbsolutePosition);
                    data[7] = (byte)msg.Speed;
                    data[8] = msg.MaxPower;
                    data[9] = (byte)msg.EndState;
                    data[10] = (byte)msg.Profile;
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