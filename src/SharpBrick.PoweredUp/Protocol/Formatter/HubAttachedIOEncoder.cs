using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAttachedIOEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(PoweredUpMessage message)
            => throw new NotImplementedException();

        public void Encode(PoweredUpMessage message, in Span<byte> data)
            => throw new NotImplementedException();

        public PoweredUpMessage Decode(byte hubId, in Span<byte> data)
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