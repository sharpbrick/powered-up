using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Knowledge;
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

        public ushort CalculateContentLength(LegoWirelessMessage message)
            => throw new NotImplementedException();

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        {
            var port = data[0];
            var modePointers = BitConverter.ToUInt16(new byte[] { data[2], data[1] });

            var portInfo = _knowledge.Port(hubId, port);

            // each combined value mode message specifies which mode/dataset is transferred (a data set is a part of a sensor input, e.g. green of RGB)
            // the modePointers reference to the requested mode/dataset
            var modeDataSetOfMessage = Enumerable.Range(0, 16) // iterate each position in ushort bitmask
                                        .Select(pos => (modePointers & (0x01 << pos)) > 0 ? pos : -1) // detect which mode/dataset is pointed
                                        .Where(x => x >= 0) // sort out unaffected ones
                                        .Select(i => portInfo.RequestedCombinedModeDataSets[i]) // get modate/dataset defintions requested
                                        .ToArray();

            var influencedModes = modeDataSetOfMessage.Select(md => md.Mode).Distinct().Select(m => _knowledge.PortMode(hubId, port, m));

            // create a buffer for each requested mode
            var dataBuffers = influencedModes
                    .ToDictionary(pmi => pmi.ModeIndex, pmi => new byte[pmi.NumberOfDatasets * PortValueSingleEncoder.GetLengthOfDataType(pmi)]);

            var currentDataSetInMessageIndex = 0;
            var remainingSlice = data[3..];

            while (remainingSlice.Length > 0)
            {
                // retrieve which mode and data set is currently sliced
                var modeDataSet = modeDataSetOfMessage[currentDataSetInMessageIndex];
                currentDataSetInMessageIndex++;

                // retrieve mode for data size length
                var modeInfo = _knowledge.PortMode(hubId, port, modeDataSet.Mode);
                int lengthOfDataType = PortValueSingleEncoder.GetLengthOfDataType(modeInfo);

                // cut data based on data type
                var dataSlice = remainingSlice.Slice(0, lengthOfDataType);

                // copy data into buffer position
                dataSlice.CopyTo(dataBuffers[modeInfo.ModeIndex].AsSpan()[(modeDataSet.DataSet * lengthOfDataType)..]);

                remainingSlice = remainingSlice[lengthOfDataType..];
            }

            return new PortValueCombinedModeMessage(
                port,
                influencedModes
                    .Select(mode => PortValueSingleEncoder.CreatPortValueData(mode, dataBuffers[mode.ModeIndex]))
                    .ToArray()
            );
        }

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}