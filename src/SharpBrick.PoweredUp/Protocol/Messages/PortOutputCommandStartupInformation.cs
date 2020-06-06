namespace SharpBrick.PoweredUp.Protocol.Messages
{
    public enum PortOutputCommandStartupInformation : byte
    {
        BufferIfNecessary = 0x00,
        ExecuteImmediately = 0x01,
    }
}