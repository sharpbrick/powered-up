namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortValueData(byte PortId, byte ModeIndex, PortModeInformationDataType DataType);
    public record PortValueData<TPayload>(byte PortId, byte ModeIndex, PortModeInformationDataType DataType, TPayload[] InputValues, TPayload[] SIInputValues, TPayload[] PctInputValues)
        : PortValueData(PortId, ModeIndex, DataType);
}