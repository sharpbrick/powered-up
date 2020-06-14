using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Functions
{
    public class DiscoverPorts
    {
        private readonly PoweredUpProtocol _protocol;
        private TaskCompletionSource<int> _taskCompletionSource;
        private int _stageTwoCount;
        private int _stageTwoExpected;

        public int SentMessages { get; private set; }
        public int ReceivedMessages { get; private set; }

        public ConcurrentBag<byte[]> ReceivedMessagesData { get; private set; }

        public DiscoverPorts(PoweredUpProtocol protocol)
        {
            _protocol = protocol ?? throw new System.ArgumentNullException(nameof(protocol));
        }

        public async Task ExecuteAsync(byte portFilter = 0xFF)
        {
            _taskCompletionSource = new TaskCompletionSource<int>();
            ReceivedMessagesData = new ConcurrentBag<byte[]>();
            _stageTwoCount = 0;


            SentMessages = 0;
            ReceivedMessages = 0;

            await _protocol.ReceiveMessageAsync(UpdateKnowledge);

            _stageTwoExpected = _protocol.Knowledge.Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter).Count();
            foreach (var port in _protocol.Knowledge.Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter))
            {
                if (port.IsDeviceConnected)
                {
                    await RequestPortProperties(port);
                }
            }

            await _taskCompletionSource.Task;
        }

        private async Task RequestPortProperties(PortInfo port)
        {
            await _protocol.SendMessageAsync(new PortInformationRequestMessage() { PortId = port.PortId, InformationType = PortInformationType.ModeInfo, });
            await _protocol.SendMessageAsync(new PortInformationRequestMessage() { PortId = port.PortId, InformationType = PortInformationType.PossibleModeCombinations, });

            SentMessages += 2;
        }

        private async Task RequestPortModeProperties(PortInfo port)
        {
            for (byte modexIndex = 0; modexIndex < port.Modes.Length; modexIndex++)
            {
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.Name, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.Raw, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.Pct, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.SI, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.Symbol, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.Mapping, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { PortId = port.PortId, Mode = modexIndex, InformationType = PortModeInformationType.ValueFormat, });

                SentMessages += 7;
            }

            _stageTwoCount++;
        }

        private async Task UpdateKnowledge(byte[] data, PoweredUpMessage message)
        {
            var knowledge = _protocol.Knowledge;

            var applicableMessage = KnowledgeManager.ApplyStaticProtocolKnowledge(message, knowledge);

            if (message is PortInformationForModeInfoMessage msg)
            {
                var port = knowledge.Port(msg.PortId);

                await RequestPortModeProperties(port);
            }

            if (applicableMessage)
            {
                ReceivedMessages++;
                ReceivedMessagesData.Add(data);

                if (SentMessages == ReceivedMessages && _stageTwoCount >= _stageTwoExpected)
                {
                    _taskCompletionSource.SetResult(ReceivedMessages);
                }
            }
        }

    }
}