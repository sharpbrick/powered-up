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

                result.Add(value);
                remainingSlice = remainingSlice[(1 + lengthOfDataType * modeInfo.NumberOfDatasets)..];
            }

            return new PortValueSingleMessage(result.ToArray());
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

        internal static PortValueData CreatePortValueDataSByte(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, sbyte>(dataSlice).ToArray();

            var scaledSIValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax));
            var scaledPctValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax));

            return CreatePortValueData<sbyte>(modeInfo, rawValues, scaledSIValues, scaledPctValues, f => Convert.ToSByte(f));
        }

        internal static PortValueData CreatePortValueDataInt16(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, short>(dataSlice).ToArray();

            var scaledSIValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax));
            var scaledPctValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax));

            return CreatePortValueData<short>(modeInfo, rawValues, scaledSIValues, scaledPctValues, f => Convert.ToInt16(f));
        }

        internal static PortValueData CreatePortValueDataInt32(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, int>(dataSlice).ToArray();

            var scaledSIValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax));
            var scaledPctValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax));

            return CreatePortValueData<int>(modeInfo, rawValues, scaledSIValues, scaledPctValues, f => Convert.ToInt32(f));
        }

        internal static PortValueData CreatePortValueDataSingle(PortModeInfo modeInfo, in Span<byte> dataSlice)
        {
            var rawValues = MemoryMarshal.Cast<byte, float>(dataSlice).ToArray();

            var scaledSIValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.SIMin, modeInfo.SIMax));
            var scaledPctValues = rawValues.Select(rv => Scale(rv, modeInfo.RawMin, modeInfo.RawMax, modeInfo.PctMin, modeInfo.PctMax));

            return CreatePortValueData<float>(modeInfo, rawValues, scaledSIValues, scaledPctValues, f => Convert.ToSingle(f));
        }

        internal static PortValueData CreatePortValueData<TDatasetType>(PortModeInfo modeInfo, TDatasetType[] rawValues, IEnumerable<double> scaledSIValues, IEnumerable<double> scaledPctValues, Func<double, TDatasetType> converter)
            => modeInfo switch
            {
                { DisableScaling: true, OverrideDatasetTypeToDouble: _, DisablePercentage: _ } => new PortValueData<TDatasetType, TDatasetType>(modeInfo.PortId, modeInfo.ModeIndex, modeInfo.DatasetType, rawValues,
                        rawValues.ToArray(),
                        rawValues.ToArray()),
                { DisableScaling: false, OverrideDatasetTypeToDouble: true, DisablePercentage: true } => new PortValueData<TDatasetType, double>(modeInfo.PortId, modeInfo.ModeIndex, modeInfo.DatasetType, rawValues,
                        scaledSIValues.ToArray(),
                        new double[scaledPctValues.Count()]),
                { DisableScaling: false, OverrideDatasetTypeToDouble: true, DisablePercentage: false } => new PortValueData<TDatasetType, double>(modeInfo.PortId, modeInfo.ModeIndex, modeInfo.DatasetType, rawValues,
                        scaledSIValues.ToArray(),
                        scaledPctValues.ToArray()),
                { DisableScaling: false, OverrideDatasetTypeToDouble: false, DisablePercentage: true } => new PortValueData<TDatasetType, TDatasetType>(modeInfo.PortId, modeInfo.ModeIndex, modeInfo.DatasetType, InputValues: rawValues,
                        SIInputValues: scaledSIValues.Select(converter).ToArray(),
                        PctInputValues: new TDatasetType[scaledPctValues.Count()]),
                { DisableScaling: false, OverrideDatasetTypeToDouble: false, DisablePercentage: false } => new PortValueData<TDatasetType, TDatasetType>(modeInfo.PortId, modeInfo.ModeIndex, modeInfo.DatasetType, InputValues: rawValues,
                        SIInputValues: scaledSIValues.Select(converter).ToArray(),
                        PctInputValues: scaledPctValues.Select(converter).ToArray()),
            };

        internal static double Scale(double value, double rawMin, double rawMax, double min, double max)
        {
            var positionInRawRange = (value - rawMin) / (rawMax - rawMin);

            return (positionInRawRange * (max - min)) + min;
        }

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}