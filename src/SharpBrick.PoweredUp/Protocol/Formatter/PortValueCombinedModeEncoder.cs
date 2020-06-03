using System;
using System.Collections.Generic;
using System.Linq;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortValueCombinedModeEncoder : IMessageContentEncoder
    {
        private readonly ProtocolKnowledge _knowledge;

        public PortValueCombinedModeEncoder(ProtocolKnowledge knowledge)
        {
            _knowledge = knowledge;
        }

        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(in Span<byte> data)
        {
            var result = new List<PortValueData>();

            var port = data[0];
            var modePointers = BitConverter.ToUInt16(new byte[] { data[2], data[1] });

            var modeDataSetIndices = Enumerable.Range(0, 16).Select(pos => (modePointers & (0x01 << pos)) > 0 ? pos : -1).Where(x => x >= 0).ToArray();

            var currentIndexIndex = 0;

            var remainingSlice = data.Slice(3);

            while (remainingSlice.Length > 0)
            {
                var portInfo = _knowledge.Port(port);

                var mode = portInfo.RequestedCombinedModeDataSets[modeDataSetIndices[currentIndexIndex]].Mode;

                //var mode = (byte)modeDataSetIndices[currentIndexIndex];

                var modeInfo = _knowledge.PortMode(port, mode);

                int lengthOfDataType = PortValueSingleEncoder.GetLengthOfDataType(modeInfo);

                var dataSlice = remainingSlice.Slice(0, modeInfo.NumberOfDatasets * lengthOfDataType);

                var value = PortValueSingleEncoder.CreatPortValueData(modeInfo, dataSlice);

                value.PortId = port;
                value.ModeIndex = mode;
                value.DataType = modeInfo.DatasetType;

                result.Add(value);
                remainingSlice = remainingSlice.Slice(lengthOfDataType * modeInfo.NumberOfDatasets);
                currentIndexIndex++;
            }

            return new PortValueCombinedModeMessage()
            {
                PortId = port,
                Data = result.ToArray(),
            };
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}