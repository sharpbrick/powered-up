using System;
using System.Text;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Parsers
{
    public class MessageParser
    {
        public static CommonMessageHeader ParseMessage(in Span<byte> data)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderParser.ParseCommonHeader(data);

            return ApplyCommonHeader(((MessageType)messageType) switch
            {
                MessageType.HubProperties => ParseHubProperties(data.Slice(headerLength)),
                MessageType.HubAttachedIO => ParseHubAttachedIO(data.Slice(headerLength)),
                _ => new UnknownMessage(),
            }, length, hubId, messageType, headerLength);
        }

        private static CommonMessageHeader ApplyCommonHeader(CommonMessageHeader message, ushort length, byte hubId, byte messageType, ushort headerLength)
        {
            message.Length = length;
            message.HubId = hubId;
            message.MessageType = (MessageType)messageType;

            return message;
        }

        private static HubPropertyMessage ParseHubProperties(in Span<byte> data)
        {
            HubPropertyMessage message = (HubProperty)data[0] switch
            {
                HubProperty.AdvertisingName => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.Button => new HubPropertyMessage<bool>() { Payload = (data[2] == 0x01) },
                HubProperty.FwVersion => new HubPropertyMessage<Version>() { Payload = PropertyVersionNumberEncoding.DecodeVersion(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.HwVersion => new HubPropertyMessage<Version>() { Payload = PropertyVersionNumberEncoding.DecodeVersion(BitConverter.ToInt32(data.Slice(2, 4))) },
                HubProperty.Rssi => new HubPropertyMessage<sbyte>() { Payload = unchecked((sbyte)data[2]) },
                HubProperty.BatteryVoltage => new HubPropertyMessage<byte>() { Payload = data[2] },
                HubProperty.BatteryType => new HubPropertyMessage<HubPropertyBatteryType>() { Payload = (HubPropertyBatteryType)data[2] },
                HubProperty.ManufacturerName => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.RadioFirmwareVersion => new HubPropertyMessage<string>() { Payload = Encoding.ASCII.GetString(data.Slice(2)) },
                HubProperty.LegoWirelessProtocolVersion => new HubPropertyMessage<Version>() { Payload = LwpVersionNumberEncoding.DecodeVersion(data[2], data[3]) },
                HubProperty.SystemTypeId => new HubPropertyMessage<byte>() { Payload = data[2] }, //TODO
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

        private static HubAttachedIOMessage ParseHubAttachedIO(in Span<byte> data)
        {
            byte portId = data[0];
            HubAttachedIOEvent ev = (HubAttachedIOEvent)data[1];
            HubAttachedIOMessage message = ev switch
            {
                HubAttachedIOEvent.AttachedIO => new HubAttachedIOForAttachedDeviceMessage()
                {
                    IOTypeId = (HubAttachedIOType)data[2],
                    HardwareRevision = PropertyVersionNumberEncoding.DecodeVersion(BitConverter.ToInt32(data.Slice(3, 4))),
                    SoftwareRevision = PropertyVersionNumberEncoding.DecodeVersion(BitConverter.ToInt32(data.Slice(7, 4))),
                },
                HubAttachedIOEvent.AttachedVirtualIO => new HubAttachedIOForAttachedVirtualDeviceMessage()
                {
                    IOTypeId = (HubAttachedIOType)data[2],
                    PortIdA = data[3],
                    PortIdB = data[4],
                },
                HubAttachedIOEvent.DetachedIO => new HubAttachedIOForDetachedDeviceMessage(),
            };


            message.PortId = portId;
            message.Event = ev;
            return message;
        }
    }
}