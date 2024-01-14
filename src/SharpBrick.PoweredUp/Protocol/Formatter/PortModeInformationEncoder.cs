using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortModeInformationEncoder : IMessageContentEncoder
{
    public ushort CalculateContentLength(LegoWirelessMessage message)
        => throw new NotImplementedException();

    public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
    {
        var portId = data[0];
        var mode = data[1];
        var informationType = (PortModeInformationType)data[2];
        var payload = data[3..];

        var message = informationType switch
        {
            // Here the specs say to use the length field and just pick the remainder of the bytes, no termination.
            PortModeInformationType.Name => new PortModeInformationForNameMessage(portId, mode, ParseStringIgnoringTrainingNullCharacters(payload)),
            PortModeInformationType.Raw => new PortModeInformationForRawMessage(portId, mode, RawMin: BitConverter.ToSingle(payload.Slice(0, 4)), RawMax: BitConverter.ToSingle(payload.Slice(4, 4))),
            PortModeInformationType.Pct => new PortModeInformationForPctMessage(portId, mode, PctMin: BitConverter.ToSingle(payload.Slice(0, 4)), PctMax: BitConverter.ToSingle(payload.Slice(4, 4))),
            PortModeInformationType.SI => new PortModeInformationForSIMessage(portId, mode, SIMin: BitConverter.ToSingle(payload.Slice(0, 4)), SIMax: BitConverter.ToSingle(payload.Slice(4, 4))),
            PortModeInformationType.Symbol => new PortModeInformationForSymbolMessage(portId, mode, Symbol: Encoding.ASCII.GetString(payload.Slice(0, payload.IndexOf<byte>(0x00)))),
            PortModeInformationType.Mapping => DecodeMapping(portId, mode, payload),
            PortModeInformationType.InternalUse => throw new NotImplementedException(),
            PortModeInformationType.MotorBias => throw new NotImplementedException(),
            PortModeInformationType.CapabilityBits => throw new NotImplementedException(),
            PortModeInformationType.ValueFormat => DecodeValueFormat(portId, mode, payload),
            _ => throw new NotImplementedException(),
        };

        return message;
    }

    private static string ParseStringIgnoringTrainingNullCharacters(Span<byte> data)
    {
        // Most devices pad the name with 0x00
        var firstNullCharIndex = data.IndexOf<byte>(0x00);
        var dataToDecode = data;
        if (firstNullCharIndex != -1) {
            // Trim everything after the last 0x00.
            dataToDecode = data.Slice(0, firstNullCharIndex);
        }

        return Encoding.ASCII.GetString(dataToDecode);
    }
    private PortModeInformationMessage DecodeMapping(byte portId, byte mode, in Span<byte> data)
        => new PortModeInformationForMappingMessage(
            portId,
            mode,
            InputSupportsNull: (data[0] & 0b1000_0000) > 0,
            InputSupportFunctionalMapping20: (data[0] & 0b0100_0000) > 0,
            InputAbsolute: (data[0] & 0b0001_0000) > 0,
            InputRelative: (data[0] & 0b0000_1000) > 0,
            InputDiscrete: (data[0] & 0b0000_0100) > 0,

            OutputSupportsNull: (data[1] & 0b1000_0000) > 0,
            OutputSupportFunctionalMapping20: (data[1] & 0b0100_0000) > 0,
            OutputAbsolute: (data[1] & 0b0001_0000) > 0,
            OutputRelative: (data[1] & 0b0000_1000) > 0,
            OutputDiscrete: (data[1] & 0b0000_0100) > 0
        );

    private PortModeInformationMessage DecodeValueFormat(byte portId, byte mode, in Span<byte> data)
        => new PortModeInformationForValueFormatMessage(
            portId,
            mode,
            NumberOfDatasets: data[0],
            DatasetType: (PortModeInformationDataType)data[1],
            TotalFigures: data[2],
            Decimals: data[3]
        );

    public void Encode(LegoWirelessMessage message, in Span<byte> data)
        => throw new NotImplementedException();
}
