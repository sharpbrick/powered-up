namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public record PortValueData(byte PortId, byte ModeIndex, PortModeInformationDataType DataType);
    public record PortValueData<TDatasetType, TOutputType>(byte PortId, byte ModeIndex, PortModeInformationDataType DataType, TDatasetType[] InputValues, TOutputType[] SIInputValues, TOutputType[] PctInputValues)
        : PortValueData(PortId, ModeIndex, DataType);
}