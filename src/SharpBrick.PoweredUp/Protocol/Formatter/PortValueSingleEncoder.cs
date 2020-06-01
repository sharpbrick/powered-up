using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortValueSingleEncoder : IMessageContentEncoder
    {
        private readonly ProtocolKnowledge _knowledge;

        public PortValueSingleEncoder(ProtocolKnowledge knowledge)
        {
            _knowledge = knowledge;
        }

        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(in Span<byte> data)
        {
            var remainingSlice = data;

            var result = new List<PortValueSingleMessageData>();

            while (remainingSlice.Length > 0)
            {
                var port = remainingSlice[0];
                var portInfo = _knowledge.Port(port);

                var mode = portInfo.LastFormattedPortMode;
                var modeInfo = _knowledge.PortMode(port, mode);

                var lengthOfDataType = modeInfo.DatasetType switch
                {
                    PortModeInformationDataType.SByte => 1,
                    PortModeInformationDataType.Int16 => 2,
                    PortModeInformationDataType.Int32 => 4,
                    PortModeInformationDataType.Single => 4,
                };

                PortValueSingleMessageData value = modeInfo.DatasetType switch
                {
                    PortModeInformationDataType.SByte => new PortValueSingleMessageData<sbyte>()
                    {
                        InputValues = MemoryMarshal.Cast<byte, sbyte>(remainingSlice.Slice(1, modeInfo.NumberOfDatasets * lengthOfDataType)).ToArray(),
                    },
                    PortModeInformationDataType.Int16 => new PortValueSingleMessageData<short>()
                    {
                        InputValues = MemoryMarshal.Cast<byte, short>(remainingSlice.Slice(1, modeInfo.NumberOfDatasets * lengthOfDataType)).ToArray(),
                    },
                    PortModeInformationDataType.Int32 => new PortValueSingleMessageData<int>()
                    {
                        InputValues = MemoryMarshal.Cast<byte, int>(remainingSlice.Slice(1, modeInfo.NumberOfDatasets * lengthOfDataType)).ToArray(),
                    },
                    PortModeInformationDataType.Single => new PortValueSingleMessageData<float>()
                    {
                        InputValues = MemoryMarshal.Cast<byte, float>(remainingSlice.Slice(1, modeInfo.NumberOfDatasets * lengthOfDataType)).ToArray(),
                    },
                };

                value.PortId = port;
                value.DataType = modeInfo.DatasetType;

                result.Add(value);
                remainingSlice = remainingSlice.Slice(1 + lengthOfDataType * modeInfo.NumberOfDatasets);
            }

            return new PortValueSingleMessage()
            {
                Data = result.ToArray(),
            };
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}