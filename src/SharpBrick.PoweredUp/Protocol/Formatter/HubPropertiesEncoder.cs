using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Messages;
namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class HubPropertiesEncoder
    {
        public static byte[] Encode(HubPropertyMessage message)
        {
            return null;
        }
        public static HubPropertyMessage Decode(in Span<byte> data)
        {
            HubPropertyMessage message = (HubProperty)data[0] switch
            {
                HubProperty.AdvertisingName => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.Button => new HubPropertyMessage<bool>() { Payload = (data[2] == 0x01) },
                HubProperty.FwVersion => new HubPropertyMessage<Version>() { Payload = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.HwVersion => new HubPropertyMessage<Version>() { Payload = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.Rssi => new HubPropertyMessage<sbyte>() { Payload = unchecked((sbyte)data[2]) },
                HubProperty.BatteryVoltage => new HubPropertyMessage<byte>() { Payload = data[2] },
                HubProperty.BatteryType => new HubPropertyMessage<HubPropertyBatteryType>() { Payload = (HubPropertyBatteryType)data[2] },
                HubProperty.ManufacturerName => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.RadioFirmwareVersion => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.LegoWirelessProtocolVersion => new HubPropertyMessage<Version>() { Payload = LwpVersionNumberEncoder.Decode(data[2], data[3]) },
                HubProperty.SystemTypeId => new HubPropertyMessage<SystemType>() { Payload = (SystemType)data[2] },
                HubProperty.HardwareNetworkId => new HubPropertyMessage<byte>() { Payload = data[2] },
                HubProperty.PrimaryMacAddress => new HubPropertyMessage<byte[]>() { Payload = data.Slice(2, 6).ToArray() },
                HubProperty.SecondaryMacAddress => new HubPropertyMessage<byte[]>() { Payload = data.Slice(2, 6).ToArray() },
                HubProperty.HardwareNetworkFamily => new HubPropertyMessage<byte>() { Payload = data[2] },

                _ => throw new InvalidOperationException(),
            };

            CommonMessageHeaderEncoder.DecodeAndApply(data, message);
            message.Property = (HubProperty)data[0];
            message.Operation = (HubPropertyOperation)data[1];

            return message;
        }
    }
}