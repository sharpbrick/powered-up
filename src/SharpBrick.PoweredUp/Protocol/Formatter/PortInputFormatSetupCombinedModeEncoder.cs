using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInputFormatSetupCombinedModeEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => (ushort)(message is PortInputFormatSetupCombinedModeForSetModeDataSetMessage setMessage ? 3 + setMessage.ModeDataSets.Length : 2);

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => Encode(message as PortInputFormatSetupCombinedModeMessage ?? throw new ArgumentException("Message not provided", nameof(message)), data);
        public void Encode(PortInputFormatSetupCombinedModeMessage message, in Span<byte> data)
        {
            data[0] = message.PortId;
            data[1] = (byte)message.SubCommand;

            if (message is PortInputFormatSetupCombinedModeForSetModeDataSetMessage setMessage)
            {
                data[2] = setMessage.CombinationIndex;
                for (int idx = 0; idx < setMessage.ModeDataSets.Length; idx++)
                {
                    byte flag = (byte)(
                        ((setMessage.ModeDataSets[idx].Mode & 0b0000_1111) << 4) +
                        (setMessage.ModeDataSets[idx].DataSet & 0b0000_1111)
                    );

                    data[3 + idx] = flag;
                }
            }
        }
    }
}