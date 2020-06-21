using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        private ConcurrentDictionary<byte, Port> _ports = new ConcurrentDictionary<byte, Port>();

        public IEnumerable<Port> Ports => _ports.Values;

        public Port Port(byte id) => _ports.TryGetValue(id, out var port) ? port : default;

        public void AddKnownPorts(IEnumerable<Port> knownPorts)
        {
            foreach (var port in knownPorts)
            {
                _ports[port.PortId] = port;
            }
        }

        public async Task CreateVirtualPortAsync(byte port1, byte port2)
        {
            await _protocol.SendMessageAsync(new VirtualPortSetupForConnectedMessage()
            {
                HubId = HubId,
                SubCommand = VirtualPortSubCommand.Connected,
                PortAId = port1,
                PortBId = port2,
            });
        }

        public async Task CloseVirtualPortAsync(byte virtualPort)
        {
            var port = Port(virtualPort);

            if (port is null || !port.IsVirtual)
            {
                throw new ArgumentException("Port not present or not virtual", nameof(virtualPort));
            }

            await _protocol.SendMessageAsync(new VirtualPortSetupForDisconnectedMessage()
            {
                HubId = HubId,
                SubCommand = VirtualPortSubCommand.Connected,
                PortId = virtualPort,
            });
        }

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

                    if (port.IsVirtual)
                    {
                        _ports.Remove(port.PortId, out var _);
                    }

                    break;
                case HubAttachedIOForAttachedVirtualDeviceMessage attachedVirtualDeviceMessage:
                    var createdVirtualPort = new Port(attachedVirtualDeviceMessage.PortId, "Virtual Port", false, attachedVirtualDeviceMessage.IOTypeId, true);

                    _ports[createdVirtualPort.PortId] = createdVirtualPort;

                    var deviceOnVirtualPort = DeviceFactory.CreateConnected(attachedVirtualDeviceMessage.IOTypeId, _protocol, attachedVirtualDeviceMessage.HubId, attachedVirtualDeviceMessage.PortId);

                    createdVirtualPort.AttachDevice(deviceOnVirtualPort, attachedVirtualDeviceMessage.IOTypeId);

                    break;
            }
        }
    }
}