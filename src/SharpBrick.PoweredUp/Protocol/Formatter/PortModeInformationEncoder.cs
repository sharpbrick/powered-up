using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortModeInformationEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(in Span<byte> data)
        {
            var portId = data[0];
            var mode = data[1];
            var informationType = (PortModeInformationType)data[2];
            var innerData = data.Slice(3);

            var message = informationType switch
            {
                PortModeInformationType.Name => DecodeName(innerData),
                PortModeInformationType.Raw => DecodeRaw(innerData),
                PortModeInformationType.Pct => DecodePct(innerData),
                PortModeInformationType.SI => DecodeSI(innerData),
                PortModeInformationType.Symbol => DecodeSymbol(innerData),
                PortModeInformationType.Mapping => DecodeMapping(innerData),
                PortModeInformationType.InternalUse => throw new NotImplementedException(),
                PortModeInformationType.MotorBias => throw new NotImplementedException(),
                PortModeInformationType.CapabilityBits => throw new NotImplementedException(),
                PortModeInformationType.ValueFormat => DecodeValueFormat(innerData),
                _ => throw new NotImplementedException(),
            };

            message.PortId = portId;
            message.Mode = mode;
            message.InformationType = informationType;

            return message;
        }

        private PortModeInformationMessage DecodeName(in Span<byte> data)
            => new PortModeInformationForNameMessage()
            {
                Name = Encoding.ASCII.GetString(data.Slice(0, data.IndexOf<byte>(0x00))),
            };

        private PortModeInformationMessage DecodeRaw(in Span<byte> data)
            => new PortModeInformationForRawMessage()
            {
                RawMin = BitConverter.ToSingle(data.Slice(0, 4)),
                RawMax = BitConverter.ToSingle(data.Slice(4, 4)),
            };

        private PortModeInformationMessage DecodePct(in Span<byte> data)
            => new PortModeInformationForPctMessage()
            {
                PctMin = BitConverter.ToSingle(data.Slice(0, 4)),
                PctMax = BitConverter.ToSingle(data.Slice(4, 4)),
            };

        private PortModeInformationMessage DecodeSI(in Span<byte> data)
            => new PortModeInformationForSIMessage()
            {
                SIMin = BitConverter.ToSingle(data.Slice(0, 4)),
                SIMax = BitConverter.ToSingle(data.Slice(4, 4)),
            };

        private PortModeInformationMessage DecodeSymbol(in Span<byte> data)
            => new PortModeInformationForSymbolMessage()
            {
                Symbol = Encoding.ASCII.GetString(data.Slice(0, data.IndexOf<byte>(0x00))),
            };

        private PortModeInformationMessage DecodeMapping(in Span<byte> data)
            => new PortModeInformationForMappingMessage()
            {
                InputSupportsNull = (data[0] & 0b1000_0000) > 0,
                InputSupportFunctionalMapping20 = (data[0] & 0b0100_0000) > 0,
                InputAbsolute = (data[0] & 0b0001_0000) > 0,
                InputRelative = (data[0] & 0b0000_1000) > 0,
                InputDiscrete = (data[0] & 0b0000_0100) > 0,

                OutputSupportsNull = (data[1] & 0b1000_0000) > 0,
                OutputSupportFunctionalMapping20 = (data[1] & 0b0100_0000) > 0,
                OutputAbsolute = (data[1] & 0b0001_0000) > 0,
                OutputRelative = (data[1] & 0b0000_1000) > 0,
                OutputDiscrete = (data[1] & 0b0000_0100) > 0,
            };

        private PortModeInformationMessage DecodeValueFormat(in Span<byte> data)
            => new PortModeInformationForValueFormatMessage()
            {
                NumberOfDatasets = data[0],
                DatasetType = (PortModeInformationDataType)data[1],
                TotalFigures = data[2],
                Decimals = data[3],
            };

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();
    }
}