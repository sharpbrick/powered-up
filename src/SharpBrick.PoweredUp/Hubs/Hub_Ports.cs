using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public abstract partial class Hub
    {
        private ConcurrentDictionary<byte, Port> _ports = new ConcurrentDictionary<byte, Port>();

        public IObservable<Port> PortChangeObservable { get; private set; }

        public IEnumerable<Port> Ports => _ports.Values;

        public Port Port(byte id)
            => _ports.TryGetValue(id, out var port) ? port : default;

        public Port Port(string friendlyName)
            => _ports.Values.FirstOrDefault(p => p.FriendlyName == friendlyName) ?? default;

        private void SetupOnPortChangeObservable(IObservable<LegoWirelessMessage> upstreamMessages)
        {
            PortChangeObservable = upstreamMessages
                .OfType<HubAttachedIOMessage>()
                .Select(msg => Port(msg.PortId));
        }

        private async Task ExpectedDevicesCompletedAsync()
            => await PortChangeObservable
                .Where(msg => Ports.All(p => p.ExpectedDevice == null || (p.ExpectedDevice != null && p.DeviceType == p.ExpectedDevice)))
                .FirstAsync()
                .GetAwaiter();

        protected void AddKnownPorts(IEnumerable<Port> knownPorts)
        {
            foreach (var port in knownPorts)
            {
                _ports[port.PortId] = port;
            }
        }

        /// <summary>
        /// Create a VirtualPort of two identical ports
        /// </summary>
        /// <param name="portId1"></param>
        /// <param name="portId2"></param>
        /// <returns></returns>
        public async Task<Port> CreateVirtualPortAsync(byte portId1, byte portId2)
        {
            AssertIsConnected();

            var portInfo1 = Protocol.Knowledge.Port(HubId, portId1);
            var portInfo2 = Protocol.Knowledge.Port(HubId, portId2);

            if (portInfo1.IOTypeId != portInfo2.IOTypeId)
            {
                throw new InvalidOperationException("Cannot combine devices of different IOType/DeviceType");
            }

            if (portInfo1.LogicalSynchronizableCapability == false || portInfo2.LogicalSynchronizableCapability == false)
            {
                throw new InvalidOperationException("One of the devices does not support logical synchronization");
            }

            var virtualPortAttachedMessage = await Protocol.SendMessageReceiveResultAsync<HubAttachedIOForAttachedVirtualDeviceMessage>(new VirtualPortSetupForConnectedMessage()
            {
                HubId = HubId,
                SubCommand = VirtualPortSubCommand.Connected,
                PortAId = portId1,
                PortBId = portId2,
            }, msg => msg.PortAId == portId1 && msg.PortBId == portId2);

            var port = OnHubAttachedVirtualIOMessage(virtualPortAttachedMessage);

            return port;
        }

        /// <summary>
        /// Remove the virtual port.
        /// </summary>
        /// <param name="virtualPortId"></param>
        /// <returns></returns>
        public async Task CloseVirtualPortAsync(byte virtualPortId)
        {
            AssertIsConnected();

            var port = Port(virtualPortId);

            if (port is null || !port.IsVirtual)
            {
                throw new ArgumentException("Port not present or not virtual", nameof(virtualPortId));
            }

            await Protocol.SendMessageAsync(new VirtualPortSetupForDisconnectedMessage()
            {
                HubId = HubId,
                SubCommand = VirtualPortSubCommand.Connected,
                PortId = virtualPortId,
            });
        }

        private void OnHubAttachedIOMessage(HubAttachedIOMessage hubAttachedIO)
        {
            Port port = null;
            switch (hubAttachedIO)
            {
                case HubAttachedIOForAttachedDeviceMessage attachedDeviceMessage:
                    port = Port(attachedDeviceMessage.PortId);

                    var device = _deviceFactory.CreateConnected(attachedDeviceMessage.IOTypeId, Protocol, attachedDeviceMessage.HubId, attachedDeviceMessage.PortId);

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

                // Note - HubAttachedIOForAttachedVirtualDeviceMessage is handled directly in OnHubChange not here
            }
        }

        private Port OnHubAttachedVirtualIOMessage(HubAttachedIOForAttachedVirtualDeviceMessage attachedVirtualDeviceMessage)
        {
            var createdVirtualPort = new Port(attachedVirtualDeviceMessage.PortId, "Virtual Port", false, attachedVirtualDeviceMessage.IOTypeId, true);

            _ports[createdVirtualPort.PortId] = createdVirtualPort;

            var deviceOnVirtualPort = _deviceFactory.CreateConnected(attachedVirtualDeviceMessage.IOTypeId, Protocol, attachedVirtualDeviceMessage.HubId, attachedVirtualDeviceMessage.PortId);

            createdVirtualPort.AttachDevice(deviceOnVirtualPort, attachedVirtualDeviceMessage.IOTypeId);

            return createdVirtualPort;
        }
    }
}