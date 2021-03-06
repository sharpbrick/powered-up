using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Messages;
namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubPropertiesEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
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
                HubPropertyOperation.Update => message switch
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
                _ => 0,
            };

            int length = 2 + messagePayloadLength;
            return (ushort)length;
        }

        public void Encode(LegoWirelessMessage message, in Span<byte> data)
            => Encode(message as HubPropertyMessage ?? throw new ArgumentException(nameof(message)), data);

        public void Encode(HubPropertyMessage message, Span<byte> data)
        {
            data[0] = (byte)message.Property;
            data[1] = (byte)message.Operation;

            if (message.Operation == HubPropertyOperation.Set || message.Operation == HubPropertyOperation.Update)
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
                    case HubPropertyMessage<SystemType> msg:
                        data[2] = (byte)msg.Payload;
                        break;
                    case HubPropertyMessage<byte> msg:
                        data[2] = msg.Payload;
                        break;
                }
            }
        }

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        {
            var property = (HubProperty)data[0];
            var operation = (HubPropertyOperation)data[1];

            HubPropertyMessage message = (HubProperty)data[0] switch
            {
                HubProperty.AdvertisingName => new HubPropertyMessage<string>(property, operation, Encoding.ASCII.GetString(data.Slice(2))),
                HubProperty.Button => new HubPropertyMessage<bool>(property, operation, (data[2] == 0x01)),
                HubProperty.FwVersion => new HubPropertyMessage<Version>(property, operation, VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4)))),
                HubProperty.HwVersion => new HubPropertyMessage<Version>(property, operation, VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(2, 4)))),
                HubProperty.Rssi => new HubPropertyMessage<sbyte>(property, operation, unchecked((sbyte)data[2])),
                HubProperty.BatteryVoltage => new HubPropertyMessage<byte>(property, operation, data[2]),
                HubProperty.BatteryType => new HubPropertyMessage<BatteryType>(property, operation, (BatteryType)data[2]),
                HubProperty.ManufacturerName => new HubPropertyMessage<string>(property, operation, Encoding.ASCII.GetString(data.Slice(2))),
                HubProperty.RadioFirmwareVersion => new HubPropertyMessage<string>(property, operation, Encoding.ASCII.GetString(data.Slice(2))),
                HubProperty.LegoWirelessProtocolVersion => new HubPropertyMessage<Version>(property, operation, LwpVersionNumberEncoder.Decode(data[2], data[3])),
                HubProperty.SystemTypeId => new HubPropertyMessage<SystemType>(property, operation, (SystemType)data[2]),
                HubProperty.HardwareNetworkId => new HubPropertyMessage<byte>(property, operation, data[2]),
                HubProperty.PrimaryMacAddress => new HubPropertyMessage<byte[]>(property, operation, data.Slice(2, 6).ToArray()),
                HubProperty.SecondaryMacAddress => new HubPropertyMessage<byte[]>(property, operation, data.Slice(2, 6).ToArray()),
                HubProperty.HardwareNetworkFamily => new HubPropertyMessage<byte>(property, operation, data[2]),

                _ => throw new InvalidOperationException(),
            };

            return message;
        }

    }
}