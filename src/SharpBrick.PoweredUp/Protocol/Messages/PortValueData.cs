namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortValueData
    {
        public byte PortId { get; set; }
        public byte ModeIndex { get; set; }
        public PortModeInformationDataType DataType { get; set; }
    }
    public class PortValueData<TPayload> : PortValueData
    {
        public TPayload[] InputValues { get; set; }
    }
}