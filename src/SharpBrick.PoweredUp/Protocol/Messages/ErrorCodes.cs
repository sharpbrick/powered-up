namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.9.2
    public enum ErrorCodes : byte
    {
        Ack = 0x01,
        MAck = 0x02,
        BufferOverflow = 0x03,
        Timeout = 0x04,
        CommandNotRecognized = 0x05,
        InvalidUse = 0x06,
        Overcurrent = 0x07,
        InternalError = 0x08,
    }
}