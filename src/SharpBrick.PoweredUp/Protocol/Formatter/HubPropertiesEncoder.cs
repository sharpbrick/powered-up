using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Messages;
namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubPropertiesEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => CalculateMessageLength(message as HubPropertyMessage ?? throw new ArgumentException(nameof(message)));

        public ushort CalculateMessageLength(HubPropertyMessage message)
        {
            int messagePayloadLength = message.Operation switch
            {
                HubPropertyOperation.Set => message switch
                {
                    HubPropertyMessage<string> msg => msg.Payload.Length,
                    HubPropertyMessage<byte[]> msg => msg.Payload.Length,
                    HubPropertyMessage<Version> msg => msg.Property switch
                    {
                        HubProperty.LegoWirelessProtocolVersion => 2, // special
                        _ => 4, // default version in int
                    },
                    _ => 1,
                },
                HubPropertyOperation.EnableUpdates => 0,
                HubPropertyOperation.DisableUpdates => 0,
                HubPropertyOperation.Reset => 0,
                HubPropertyOperation.RequestUpdate => 0,
                HubPropertyOperation.Update => throw new NotImplementedException(),
                _ => 0,
            };

            int length = 2 + messagePayloadLength;
            return (ushort)length;
        }

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => Encode(message as HubPropertyMessage ?? throw new ArgumentException(nameof(message)), data);

        public void Encode(HubPropertyMessage message, Span<byte> data)
        {
            data[0] = (byte)message.Property;
            data[1] = (byte)message.Operation;

            if (message.Operation == HubPropertyOperation.Set)
            {
                switch (message)
                {
                    case HubPropertyMessage<string> msg:
                        byte[] stringAsBytes = Encoding.ASCII.GetBytes(msg.Payload);
                        stringAsBytes.CopyTo(data.Slice(2, stringAsBytes.Length));
                        break;
                    case HubPropertyMessage<bool> msg:
                        throw new NotImplementedException(); //data[2] = (byte)(msg.Payload ? 0x01 : 0x00);
                    case HubPropertyMessage<Version> msg:
                        throw new NotImplementedException();
                    case HubPropertyMessage<sbyte> msg:
                        throw new NotImplementedException(); //data[2] = (byte)msg.Payload;
                    case HubPropertyMessage<byte> msg:
                        data[2] = msg.Payload;
                        break;
                }
            }
        }

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
        {
            HubPropertyMessage message = (HubProperty)data[0] switch
            {
                HubProperty.AdvertisingName => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.Button => new HubPropertyMessage<bool>() { Payload = (data[2] == 0x01) },
                HubProperty.FwVersion => new HubPropertyMessage<Version>() { Payload = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.HwVersion => new HubPropertyMessage<Version>() { Payload = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.Rssi => new HubPropertyMessage<sbyte>() { Payload = unchecked((sbyte)data[2]) },
                HubProperty.BatteryVoltage => new HubPropertyMessage<byte>() { Payload = data[2] },
                HubProperty.BatteryType => new HubPropertyMessage<BatteryType>() { Payload = (BatteryType)data[2] },
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

            message.Property = (HubProperty)data[0];
            message.Operation = (HubPropertyOperation)data[1];

            return message;
        }

    }
}