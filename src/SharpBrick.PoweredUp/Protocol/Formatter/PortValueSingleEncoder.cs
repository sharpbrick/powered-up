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

            var result = new List<PortValueData>();

            while (remainingSlice.Length > 0)
            {
                var port = remainingSlice[0];
                var portInfo = _knowledge.Port(port);

                var mode = portInfo.LastFormattedPortMode;
                var modeInfo = _knowledge.PortMode(port, mode);

                int lengthOfDataType = PortValueSingleEncoder.GetLengthOfDataType(modeInfo);

                var dataSlice = remainingSlice.Slice(1, modeInfo.NumberOfDatasets * lengthOfDataType);

                var value = PortValueSingleEncoder.CreatPortValueData(modeInfo, dataSlice);

                value.PortId = port;
                value.DataType = modeInfo.DatasetType;
                value.ModeIndex = mode;

                result.Add(value);
                remainingSlice = remainingSlice.Slice(1 + lengthOfDataType * modeInfo.NumberOfDatasets);
            }

            return new PortValueSingleMessage()
            {
                Data = result.ToArray(),
            };
        }

        internal static int GetLengthOfDataType(PortModeInfo modeInfo)
            => modeInfo.DatasetType switch
            {
                PortModeInformationDataType.SByte => 1,
                PortModeInformationDataType.Int16 => 2,
                PortModeInformationDataType.Int32 => 4,
                PortModeInformationDataType.Single => 4,
                _ => throw new NotSupportedException(),
            };

        internal static PortValueData CreatPortValueData(PortModeInfo modeInfo, Span<byte> dataSlice)
        {
            return modeInfo.DatasetType switch
            {
                PortModeInformationDataType.SByte => new PortValueData<sbyte>()
                {
                    InputValues = MemoryMarshal.Cast<byte, sbyte>(dataSlice).ToArray(),
                },
                PortModeInformationDataType.Int16 => new PortValueData<short>()
                {
                    InputValues = MemoryMarshal.Cast<byte, short>(dataSlice).ToArray(),
                },
                PortModeInformationDataType.Int32 => new PortValueData<int>()
                {
                    InputValues = MemoryMarshal.Cast<byte, int>(dataSlice).ToArray(),
                },
                PortModeInformationDataType.Single => new PortValueData<float>()
                {
                    InputValues = MemoryMarshal.Cast<byte, float>(dataSlice).ToArray(),
                },

                _ => throw new NotSupportedException(),
            };
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}