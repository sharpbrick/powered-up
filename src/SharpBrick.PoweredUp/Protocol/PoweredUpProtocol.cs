using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public class PoweredUpProtocol
    {
        private readonly BluetoothKernel _kernel;
        public ProtocolKnowledge Knowledge = new ProtocolKnowledge();

        public PoweredUpProtocol(BluetoothKernel kernel)
        {
            _kernel = kernel;
        }
        public async Task SendMessageAsync(PoweredUpMessage message)
        {
            var knowledge = Knowledge;

            var data = MessageEncoder.Encode(message, knowledge);

            await _kernel.SendBytesAsync(data);
        }

        public async Task ReceiveMessageAsync(Func<PoweredUpMessage, Task> handler)
        {
            await _kernel.ReceiveBytesAsync(async data =>
            {
                var knowledge = Knowledge;

                var message = MessageEncoder.Decode(data, knowledge);

                await UpdateProtocolKnowledge(message);

                await handler(message);
            });
        }

        private Task UpdateProtocolKnowledge(PoweredUpMessage message)
        {
            PortInfo port;
            PortModeInfo mode;
            switch (message)
            {
                case HubAttachedIOForAttachedDeviceMessage msg:
                    port = Knowledge.Port(msg.PortId);

                    ResetProtocolKnowledgeForPort(port.PortId);
                    port.IsDeviceConnected = true;
                    port.IOTypeId = msg.IOTypeId;
                    port.HardwareRevision = msg.HardwareRevision;
                    port.SoftwareRevision = msg.SoftwareRevision;

                    AddCachePortAndPortModeInformation(msg.IOTypeId, port);
                    break;
                case HubAttachedIOForDetachedDeviceMessage msg:
                    port = Knowledge.Port(msg.PortId);

                    ResetProtocolKnowledgeForPort(port.PortId);
                    port.IsDeviceConnected = false;
                    break;

                case PortInputFormatSingleMessage msg:
                    port = Knowledge.Port(msg.PortId);
                    mode = Knowledge.PortMode(msg.PortId, msg.Mode);

                    port.LastFormattedPortMode = msg.Mode;

                    mode.DeltaInterval = msg.DeltaInterval;
                    mode.NotificationEnabled = msg.NotificationEnabled;
                    break;

                case PortInputFormatCombinedModeMessage msg:
                    port = Knowledge.Port(msg.PortId);

                    port.UsedCombinationIndex = msg.UsedCombinationIndex;
                    port.MultiUpdateEnabled = msg.MultiUpdateEnabled;
                    port.ConfiguredModeDataSetIndex = msg.ConfiguredModeDataSetIndex;
                    break;
            }

            return Task.CompletedTask;
        }

        private void AddCachePortAndPortModeInformation(HubAttachedIOType type, PortInfo port)
        {
            var device = DeviceFactory.Create(type);

            device.ApplyStaticPortInfo(port);
        }

        private void ResetProtocolKnowledgeForPort(byte portId)
        {
            var port = Knowledge.Port(portId);

            port.IsDeviceConnected = false;
            port.IOTypeId = HubAttachedIOType.Unknown;
            port.HardwareRevision = new Version("0.0.0.0");
            port.SoftwareRevision = new Version("0.0.0.0");

            port.OutputCapability = false;
            port.InputCapability = false;
            port.LogicalCombinableCapability = false;
            port.LogicalSynchronizableCapability = false;
            port.Modes = Array.Empty<PortModeInfo>();

            port.ModeCombinations = Array.Empty<ushort>();

            port.UsedCombinationIndex = 0;
            port.MultiUpdateEnabled = false;
            port.ConfiguredModeDataSetIndex = Array.Empty<int>();
        }
    }
}