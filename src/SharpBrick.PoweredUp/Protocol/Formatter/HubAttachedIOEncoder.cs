using System;
using SharpBrick.PoweredUp.Protocol.Messages;
namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public static class HubAttachedIOEncoder
    {
        public static HubAttachedIOMessage Decode(in Span<byte> data)
        {
            byte portId = data[0];
            HubAttachedIOEvent ev = (HubAttachedIOEvent)data[1];
            HubAttachedIOMessage message = ev switch
            {
                HubAttachedIOEvent.AttachedIO => new HubAttachedIOForAttachedDeviceMessage()
                {
                    IOTypeId = (HubAttachedIOType)data[2],
                    HardwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(3, 4))),
                    SoftwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(7, 4))),
                },
                HubAttachedIOEvent.AttachedVirtualIO => new HubAttachedIOForAttachedVirtualDeviceMessage()
                {
                    IOTypeId = (HubAttachedIOType)data[2],
                    PortIdA = data[3],
                    PortIdB = data[4],
                },
                HubAttachedIOEvent.DetachedIO => new HubAttachedIOForDetachedDeviceMessage(),
            };

            CommonMessageHeaderEncoder.DecodeAndApply(data, message);
            message.PortId = portId;
            message.Event = ev;

            return message;
        }
    }
}