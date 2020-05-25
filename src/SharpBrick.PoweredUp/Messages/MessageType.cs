namespace SharpBrick.PoweredUp.Messages
{
    // spec chapter: 3.3
    public enum MessageType : byte
    {
        // Hub Related
        HubProperties = 0x01,
        HubActions = 0x02,
        HubAlerts = 0x03,
        HubAttachedIO = 0x04,
        GenericErrorMessages = 0x05,
        HwNetWorkCommands = 0x08,
        FwUpdateGoIntoBootMode = 0x10,
        FwUpdateLockMemory = 0x11,
        FwUpdateLockStatusRequest = 0x12,
        FwLockStatus = 0x13,

        // Port Related
        PortInformationRequest = 0x21,
        PortModeInformationRequest = 0x22,
        PortInputFormatSetupSingle = 0x41,
        PortInputFormatSetupCombinedMode = 0x42,
        PortInformation = 0x43,
        PortModeInformation = 0x44,
        PortValueSingle = 0x45,
        PortValueCombinedMode = 0x46,
        PortInputFormatSingle = 0x47,
        PortInputFormatCombinedMode = 0x48,
        VirtualPortSetup = 0x61,
        PortOutputCommand = 0x81,
        PortOutputCommandFeedback = 0x82,
    }
}
