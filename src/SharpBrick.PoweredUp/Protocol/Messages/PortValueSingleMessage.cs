namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public class PortValueSingleMessage : PoweredUpMessage
    {
        public PortValueSingleMessageData[] Data { get; set; }
    }

    public class PortValueSingleMessageData
    {
        public byte PortId { get; set; }
        public PortModeInformationDataType DataType { get; set; }
    }
    public class PortValueSingleMessageData<TPayload> : PortValueSingleMessageData
    {
        public TPayload[] InputValues { get; set; }
    }
}