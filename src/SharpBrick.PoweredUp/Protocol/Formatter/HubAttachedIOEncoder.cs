using System;
using SharpBrick.PoweredUp.Protocol.Messages;
namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class HubAttachedIOEncoder : IMessageContentEncoder
    {
        public ushort CalculateContentLength(CommonMessageHeader message)
            => throw new NotImplementedException();

        public void Encode(CommonMessageHeader message, in Span<byte> data)
            => throw new NotImplementedException();

        public CommonMessageHeader Decode(in Span<byte> data)
        {
            byte portId = data[0];
            HubAttachedIOEvent ev = (HubAttachedIOEvent)data[1];
            HubAttachedIOMessage message = ev switch
            {
                HubAttachedIOEvent.AttachedIO => new HubAttachedIOForAttachedDeviceMessage()
                {
                    IOTypeId = (HubAttachedIOType)BitConverter.ToUInt16(data.Slice(2, 2)),
                    HardwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(4, 4))),
                    SoftwareRevision = VersionNumberEncoder.Decode(BitConverter.ToInt32(data.Slice(8, 4))),
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