namespace SharpBrick.PoweredUp.Protocol.Messages;

public enum PortInputFormatSetupCombinedSubCommand : byte
{
    SetModeAndDataSetCombination = 0x01,
    LockDeviceForSetup = 0x02,
    UnlockAndStartWithMultiUpdateEnabled = 0x03,
    UnlockAndStartWithMultiUpdateDisabled = 0x04,
    ResetSensor = 0x06,
}
