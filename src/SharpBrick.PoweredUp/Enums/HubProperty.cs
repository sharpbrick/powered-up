namespace SharpBrick.PoweredUp;

// spec chapter: 3.5.3
public enum HubProperty : byte
{
    AdvertisingName = 0x01,
    Button = 0x02,
    FwVersion = 0x03,
    HwVersion = 0x04,
    Rssi = 0x05,
    BatteryVoltage = 0x06,
    BatteryType = 0x07,
    ManufacturerName = 0x08,
    RadioFirmwareVersion = 0x09,
    LegoWirelessProtocolVersion = 0x0A,
    SystemTypeId = 0x0B,
    HardwareNetworkId = 0x0C,
    PrimaryMacAddress = 0x0D,
    SecondaryMacAddress = 0x0E,
    HardwareNetworkFamily = 0x0F,
}
