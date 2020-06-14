using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        public string AdvertisingName { get; private set; }
        public bool Button { get; private set; }
        public Version FirmwareVersion { get; private set; }
        public Version HardwareVersion { get; private set; }
        public sbyte Rssi { get; private set; }
        public byte BatteryVoltageInPercent { get; private set; }
        public BatteryType BatteryType { get; private set; }
        public string ManufacturerName { get; private set; }
        public string RadioFirmwareVersion { get; private set; }
        public Version LegoWirelessProtocolVersion { get; private set; }
        public SystemType SystemType { get; private set; }
        public byte HardwareNetworkId { get; private set; }
        public byte[] PrimaryMacAddress { get; private set; }
        public byte[] SecondaryMacAddress { get; private set; }
        public byte HardwareNetworkFamily { get; private set; }


        private async Task InitialHubPropertiesQueryAsync()
        {
            await RequestHubPropertySingleUpdate(HubProperty.AdvertisingName);
            await RequestHubPropertySingleUpdate(HubProperty.Button);
            await RequestHubPropertySingleUpdate(HubProperty.FwVersion);
            await RequestHubPropertySingleUpdate(HubProperty.HwVersion);
            await RequestHubPropertySingleUpdate(HubProperty.Rssi);
            await RequestHubPropertySingleUpdate(HubProperty.BatteryVoltage);
            await RequestHubPropertySingleUpdate(HubProperty.BatteryType);
            await RequestHubPropertySingleUpdate(HubProperty.ManufacturerName);
            await RequestHubPropertySingleUpdate(HubProperty.RadioFirmwareVersion);
            await RequestHubPropertySingleUpdate(HubProperty.LegoWirelessProtocolVersion);
            await RequestHubPropertySingleUpdate(HubProperty.SystemTypeId);
            await RequestHubPropertySingleUpdate(HubProperty.HardwareNetworkId);
            await RequestHubPropertySingleUpdate(HubProperty.PrimaryMacAddress);
            await RequestHubPropertySingleUpdate(HubProperty.SecondaryMacAddress);
            await RequestHubPropertySingleUpdate(HubProperty.HardwareNetworkFamily);
        }

        public Task RequestHubPropertySingleUpdate(HubProperty property)
            => _protocol.SendMessageAsync(new HubPropertyMessage() { Property = property, Operation = HubPropertyOperation.RequestUpdate });

        private void OnHubPropertyMessage(HubPropertyMessage hubProperty)
        {
            if (hubProperty.Property == HubProperty.AdvertisingName && hubProperty is HubPropertyMessage<string> advData)
            {
                AdvertisingName = advData.Payload;
            }
            else if (hubProperty.Property == HubProperty.Button && hubProperty is HubPropertyMessage<bool> btnData)
            {
                Button = btnData.Payload;
            }
            else if (hubProperty.Property == HubProperty.FwVersion && hubProperty is HubPropertyMessage<Version> fwVersionData)
            {
                FirmwareVersion = fwVersionData.Payload;
            }
            else if (hubProperty.Property == HubProperty.HwVersion && hubProperty is HubPropertyMessage<Version> hwVersionData)
            {
                HardwareVersion = hwVersionData.Payload;
            }
            else if (hubProperty.Property == HubProperty.Rssi && hubProperty is HubPropertyMessage<sbyte> rssiData)
            {
                Rssi = rssiData.Payload;
            }
            else if (hubProperty.Property == HubProperty.BatteryVoltage && hubProperty is HubPropertyMessage<byte> batteryVoltageData)
            {
                BatteryVoltageInPercent = batteryVoltageData.Payload;
            }
            else if (hubProperty.Property == HubProperty.BatteryVoltage && hubProperty is HubPropertyMessage<BatteryType> batteryTypeData)
            {
                BatteryType = batteryTypeData.Payload;
            }
            else if (hubProperty.Property == HubProperty.ManufacturerName && hubProperty is HubPropertyMessage<string> manNameData)
            {
                ManufacturerName = manNameData.Payload;
            }
            else if (hubProperty.Property == HubProperty.RadioFirmwareVersion && hubProperty is HubPropertyMessage<string> radioData)
            {
                RadioFirmwareVersion = radioData.Payload;
            }
            else if (hubProperty.Property == HubProperty.LegoWirelessProtocolVersion && hubProperty is HubPropertyMessage<Version> lwpData)
            {
                LegoWirelessProtocolVersion = lwpData.Payload;
            }
            else if (hubProperty.Property == HubProperty.SystemTypeId && hubProperty is HubPropertyMessage<SystemType> systemData)
            {
                SystemType = systemData.Payload;
            }
            else if (hubProperty.Property == HubProperty.HardwareNetworkId && hubProperty is HubPropertyMessage<byte> hwNetData)
            {
                HardwareNetworkId = hwNetData.Payload;
            }
            else if (hubProperty.Property == HubProperty.PrimaryMacAddress && hubProperty is HubPropertyMessage<byte[]> primaryData)
            {
                PrimaryMacAddress = primaryData.Payload;
            }
            else if (hubProperty.Property == HubProperty.SecondaryMacAddress && hubProperty is HubPropertyMessage<byte[]> secondaryData)
            {
                SecondaryMacAddress = secondaryData.Payload;
            }
            else if (hubProperty.Property == HubProperty.HardwareNetworkFamily && hubProperty is HubPropertyMessage<byte> hwNetFamilyData)
            {
                HardwareNetworkFamily = hwNetFamilyData.Payload;
            }
        }
    }
}