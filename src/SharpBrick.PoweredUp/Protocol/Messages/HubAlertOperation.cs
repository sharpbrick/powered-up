namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.7.3
public enum HubAlertOperation : byte
{
    EnableUpdates = 0x01, // (Downstream)
    DisableUpdates = 0x02, // (Downstream)
    RequestUpdates = 0x03, //(Downstream)
    Update = 0x04, // (Upstream)
}
