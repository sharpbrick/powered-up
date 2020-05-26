namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.8.3
    public enum HubAttachedIOEvent : byte
    {
        DetachedIO = 0x00,
        AttachedIO = 0x01,
        AttachedVirtualIO = 0x02,
    }
}