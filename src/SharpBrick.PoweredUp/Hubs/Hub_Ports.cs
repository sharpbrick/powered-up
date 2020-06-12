using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        private Port[] _ports;

        public IEnumerable<Port> Ports => _ports;

        public Port Port(byte id) => _ports.FirstOrDefault(p => p.PortId == id);

        private void OnHubAttachedIOMessage(HubAttachedIOMessage hubAttachedIO)
        {
            Port port = null;
            switch (hubAttachedIO)
            {
                case HubAttachedIOForAttachedDeviceMessage attachedDeviceMessage:
                    port = Port(attachedDeviceMessage.PortId);

                    var device = DeviceFactory.CreateConnected(attachedDeviceMessage.IOTypeId, _protocol, attachedDeviceMessage.HubId, attachedDeviceMessage.PortId);

                    port.AttachDevice(device, attachedDeviceMessage.IOTypeId);
                    break;
                case HubAttachedIOForDetachedDeviceMessage detachedDeviceMessage:
                    port = Port(detachedDeviceMessage.PortId);

                    port.DetachDevice();
                    break;
                case HubAttachedIOForAttachedVirtualDeviceMessage attachedVirtualDeviceMessage:
                    throw new NotImplementedException();
                    break;
            }
        }
    }
}