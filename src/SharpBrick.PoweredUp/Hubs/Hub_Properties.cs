using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
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

        public Task SetAdvertisingNameAsync(string advertisingName)
            => SetHubPropertyAsync<string>(HubProperty.AdvertisingName, advertisingName);
        public Task SetHardwareNetworkIdAsync(byte hardwareNetworkId)
            => SetHubPropertyAsync<byte>(HubProperty.HardwareNetworkId, hardwareNetworkId);
        public Task SetHardwareNetworkFamilyAsync(byte hardwareNetworkFamily)
            => SetHubPropertyAsync<byte>(HubProperty.HardwareNetworkFamily, hardwareNetworkFamily);
        public Task ResetAdvertisingNameAsync()
            => ResetHubPropertyAsync(HubProperty.AdvertisingName);
        public Task ResetHardwareNetworkIdAsync()
            => ResetHubPropertyAsync(HubProperty.HardwareNetworkId);


        private async Task InitialHubPropertiesQueryAsync()
        {
            await Task.WhenAll(
                RequestHubPropertySingleUpdate(HubProperty.AdvertisingName),
                RequestHubPropertySingleUpdate(HubProperty.Button),
                RequestHubPropertySingleUpdate(HubProperty.FwVersion),
                RequestHubPropertySingleUpdate(HubProperty.HwVersion),
                RequestHubPropertySingleUpdate(HubProperty.Rssi),
                RequestHubPropertySingleUpdate(HubProperty.BatteryVoltage),
                RequestHubPropertySingleUpdate(HubProperty.BatteryType),
                RequestHubPropertySingleUpdate(HubProperty.ManufacturerName),
                RequestHubPropertySingleUpdate(HubProperty.RadioFirmwareVersion),
                RequestHubPropertySingleUpdate(HubProperty.LegoWirelessProtocolVersion),
                RequestHubPropertySingleUpdate(HubProperty.SystemTypeId),
                RequestHubPropertySingleUpdate(HubProperty.HardwareNetworkId),
                RequestHubPropertySingleUpdate(HubProperty.PrimaryMacAddress),
                RequestHubPropertySingleUpdate(HubProperty.SecondaryMacAddress)
            // RequestHubPropertySingleUpdate(HubProperty.HardwareNetworkFamily) does not work .. at least for TechnicMediumHub. Throws command not recognized error.
            );
        }

        public Task RequestHubPropertySingleUpdate(HubProperty property)
            => Protocol.SendMessageReceiveResultAsync<HubPropertyMessage>(new HubPropertyMessage()
            {
                HubId = HubId,
                Property = property,
                Operation = HubPropertyOperation.RequestUpdate
            }, msg => msg.Operation == HubPropertyOperation.Update && msg.Property == property);

        public async Task SetHubPropertyAsync<T>(HubProperty property, T value)
        {
            if (property != HubProperty.AdvertisingName &&
                property != HubProperty.HardwareNetworkFamily &&
                property != HubProperty.HardwareNetworkId
                )
            {
                throw new ArgumentException("Not all properties can be set (only AdvertisingName, HardwareNetworkFamily, HardwareNetworkId)", nameof(property));
            }

            await Protocol.SendMessageAsync(new HubPropertyMessage<T>()
            {
                HubId = HubId,
                Operation = HubPropertyOperation.Set,
                Property = property,
                Payload = value,
            });

            await RequestHubPropertySingleUpdate(property);
        }

        public async Task ResetHubPropertyAsync(HubProperty property)
        {
            if (property != HubProperty.AdvertisingName &&
                property != HubProperty.HardwareNetworkId
                )
            {
                throw new ArgumentException("Not all properties can be reset (only AdvertisingName, HardwareNetworkId)", nameof(property));
            }

            await Protocol.SendMessageAsync(new HubPropertyMessage()
            {
                HubId = HubId,
                Operation = HubPropertyOperation.Reset,
                Property = property,
            });

            await RequestHubPropertySingleUpdate(property);
        }

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