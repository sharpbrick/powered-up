using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpBrick.PoweredUp.Protocol.Knowledge;
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

        public ushort CalculateContentLength(LegoWirelessMessage message)
            => throw new NotImplementedException();

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        {
            var remainingSlice = data;

            var result = new List<PortValueData>();

            while (remainingSlice.Length > 0)
            {
                var port = remainingSlice[0];
                var portInfo = _knowledge.Port(hubId, port);

                var mode = portInfo.LastFormattedPortMode;
                var modeInfo = _knowledge.PortMode(hubId, port, mode);

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

        internal static PortValueData CreatPortValueData(PortModeInfo modeInfo, in Span<byte> dataSlice)
            => modeInfo.DatasetType switch
            {
                PortModeInformationDataType.SByte => CreatePortValueDataSByte(modeInfo, dataSlice),
                PortModeInformationDataType.Int16 => CreatePortValueDataInt16(modeInfo, dataSlice),
                PortModeInformationDataType.Int32 => CreatePortValueDataInt32(modeInfo, dataSlice),
                PortModeInformationDataType.Single => CreatePortValueDataSingle(modeInfo, dataSlice),

                _ => throw new NotSupportedException(),
            };

        internal static PortValueData<sbyte> CreatePortValueDataSByte(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, sbyte>(dataSlice).ToArray();

            var siValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax)).Select(f => Convert.ToSByte(f)).ToArray();
            var pctValues = modeInfo.DisablePercentage switch
            {
                false => rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax)).Select(f => Convert.ToSByte(f)).ToArray(),
                true => new sbyte[rawValues.Length],
            };

            return new PortValueData<sbyte>()
            {
                InputValues = rawValues,
                SIInputValues = siValues,
                PctInputValues = pctValues,
            };
        }

        internal static PortValueData<short> CreatePortValueDataInt16(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, short>(dataSlice).ToArray();

            var siValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax)).Select(f => Convert.ToInt16(f)).ToArray();
            var pctValues = modeInfo.DisablePercentage switch
            {
                false => rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax)).Select(f => Convert.ToInt16(f)).ToArray(),
                true => new short[rawValues.Length],
            };

            return new PortValueData<short>()
            {
                InputValues = rawValues,
                SIInputValues = siValues,
                PctInputValues = pctValues,
            };
        }

        internal static PortValueData<int> CreatePortValueDataInt32(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, int>(dataSlice).ToArray();

            var siValues = modeInfo switch
            {
                { DisableScaling: true } => rawValues,
                _ => rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax)).Select(f => Convert.ToInt32(f)).ToArray(),
            };

            var pctValues = modeInfo switch
            {
                { DisablePercentage: true } => new int[rawValues.Length],
                { DisableScaling: true } => rawValues,
                _ => rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax)).Select(f => Convert.ToInt32(f)).ToArray(),
            };

            return new PortValueData<int>()
            {
                InputValues = rawValues,
                SIInputValues = siValues,
                PctInputValues = pctValues,
            };
        }

        internal static PortValueData<float> CreatePortValueDataSingle(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, float>(dataSlice).ToArray();

            var siValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax)).ToArray();
            var pctValues = modeInfo.DisablePercentage switch
            {
                false => rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax)).ToArray(),
                true => new float[rawValues.Length],
            };

            return new PortValueData<float>()
            {
                InputValues = rawValues,
                SIInputValues = siValues,
                PctInputValues = pctValues,
            };
        }

        internal static float Scale(float value, float rawMin, float rawMax, float min, float max)
        {
            var positionInRawRange = (value - rawMin) / (rawMax - rawMin);

            return (positionInRawRange * (max - min)) + min;
        }

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}