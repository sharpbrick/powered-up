using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAttachedIOEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(LegoWirelessMessage message)
            => message switch
            {
                HubAttachedIOForAttachedDeviceMessage _ => 12,
                HubAttachedIOForDetachedDeviceMessage _ => 2,
                HubAttachedIOForAttachedVirtualDeviceMessage _ => 6,
                _ => 0,
            };


        public void Encode(LegoWirelessMessage message, in Span<byte> data)
        {
            switch (message)
            {
                case HubAttachedIOForAttachedDeviceMessage x:
                    Encode(x, data);
                    break;
                case HubAttachedIOForDetachedDeviceMessage x:
                    Encode(x, data);
                    break;
                case HubAttachedIOForAttachedVirtualDeviceMessage x:
                    Encode(x, data);
                    break;
            };
        }

        public void Encode(HubAttachedIOForAttachedDeviceMessage message, in Span<byte> data)
        {
            data[0] = message.PortId;
            data[1] = (byte)message.Event;
            BitConverter.TryWriteBytes(data.Slice(2, 2), (ushort)message.IOTypeId);
            BitConverter.TryWriteBytes(data.Slice(4, 4), VersionNumberEncoder.Encode(message.HardwareRevision));
            BitConverter.TryWriteBytes(data.Slice(8, 4), VersionNumberEncoder.Encode(message.SoftwareRevision));
        }

        public void Encode(HubAttachedIOForDetachedDeviceMessage message, in Span<byte> data)
            => throw new NotImplementedException();

        public void Encode(HubAttachedIOForAttachedVirtualDeviceMessage message, in Span<byte> data)
        {
            data[0] = message.PortId;
            data[1] = (byte)message.Event;
            BitConverter.TryWriteBytes(data.Slice(2, 2), (ushort)message.IOTypeId);
            data[4] = message.PortAId;
            data[5] = message.PortBId;
        }

        public LegoWirelessMessage Decode(byte hubId, in Span<byte> data)
        {
            byte portId = data[0];
            HubAttachedIOEvent ev = (HubAttachedIOEvent)data[1];
            HubAttachedIOMessage message = ev switch
            {
                HubAttachedIOEvent.AttachedIO => new HubAttachedIOForAttachedDeviceMessage()
                {
                    IOTypeId = (DeviceType)BitConverter.ToUInt16(data.Slice(2, 2)),
                    HardwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(4, 4))),
                    SoftwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(8, 4))),
                },
                HubAttachedIOEvent.AttachedVirtualIO => new HubAttachedIOForAttachedVirtualDeviceMessage()
                {
                    IOTypeId = (DeviceType)BitConverter.ToUInt16(data.Slice(2, 2)),
                    PortAId = data[4],
                    PortBId = data[5],
                },
                HubAttachedIOEvent.DetachedIO => new HubAttachedIOForDetachedDeviceMessage(),
                _ => throw new NotImplementedException(),
            };

            message.PortId = portId;
            message.Event = ev;

            return message;
        }

    }
}