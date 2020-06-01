namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public enum PortModeInformationDataType : byte
    {
        SByte = 0x00, // UNSPECED whether byte or sbyte
        Int16 = 0x01, // UNSPECED whether UInt16 or Int16
        Int32 = 0x02, // UNSPECED whether UInt32 or Int32
        Single = 0x03,
    }
}