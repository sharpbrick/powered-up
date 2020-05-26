namespace SharpBrick.PoweredUp.Protocol.Messages
{
    // spec chapter: 3.5.4
    public enum HubPropertyOperation : byte
    {
        Set = 0x01, // Set (Downstream)
        EnableUpdates = 0x02, // Enable Updates (Downstream)
        DisableUpdates = 0x03, // Disable Updates (Downstream)
        Reset = 0x04, // Reset (Downstream)
        RequestUpdate = 0x05, // Request Update (Downstream)
        Update = 0x06, // Update (Upstream)
    }
}