namespace SharpBrick.PoweredUp.Protocol.Messages;

// spec chapter: 3.16.2
public enum PortModeInformationType : byte
{
    Name = 0x00, // Mode Name
    Raw = 0x01, // Raw Range
    Pct = 0x02, // Percent Range
    SI = 0x03, // SI Value Range
    Symbol = 0x04, // Standard Name of Value
    Mapping = 0x05,
    InternalUse = 0x06,
    MotorBias = 0x07, // 0-100%
    CapabilityBits = 0x08, // 6 bytes
    ValueFormat = 0x80, // Value Encoding
}
