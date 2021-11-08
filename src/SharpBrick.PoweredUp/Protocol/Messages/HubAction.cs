namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.6.2
public enum HubAction : byte
{
    // Downstream
    SwitchOffHub = 0x01,
    Disconnect = 0x02,
    VccPortControlOn = 0x03,
    VccPortControlOff = 0x04,
    ActivateBusyIndication = 0x05,
    ResetBusyIndication = 0x06,
    ProductionShutdown = 0X2F,

    // Upstream
    HubWillSwitchOff = 0x30,
    HubWillDisconnect = 0x31,
    HubWillGoIntoBootMode = 0x32,
}
