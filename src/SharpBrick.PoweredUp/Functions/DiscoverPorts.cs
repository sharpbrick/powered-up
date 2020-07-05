using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Functions
{
    public class DiscoverPorts
    {
        private object lockObject = new object();

        private readonly IPoweredUpProtocol _protocol;
        private readonly byte _hubId;
        private readonly ILogger<DiscoverPorts> _logger;
        private readonly IDisposable _disposable;
        private TaskCompletionSource<int> _taskCompletionSource;
        private int _stageTwoCount;
        private int _stageTwoExpected;

        public int SentMessages { get; private set; }
        public int ReceivedMessages { get; private set; }

        public ConcurrentBag<byte[]> ReceivedMessagesData { get; private set; }

        public DiscoverPorts(IPoweredUpProtocol protocol, byte hubId = 0, ILogger<DiscoverPorts> logger = default)
        {
            _protocol = protocol ?? throw new System.ArgumentNullException(nameof(protocol));
            _hubId = hubId;
            _logger = logger;
        }

        public async Task ExecuteAsync(byte portFilter = 0xFF)
        {
            _taskCompletionSource = new TaskCompletionSource<int>();
            ReceivedMessagesData = new ConcurrentBag<byte[]>();
            _stageTwoCount = 0;

            SentMessages = 0;
            ReceivedMessages = 0;

            using var disposable = _protocol.UpstreamRawMessages.Subscribe(tuple => UpdateKnowledge(tuple.data, tuple.message));

            _stageTwoExpected = _protocol.Knowledge.Hub(_hubId).Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter).Count();

            Console.WriteLine($"Number of Ports: {_stageTwoExpected}");

            foreach (var port in _protocol.Knowledge.Hub(_hubId).Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter))
            {
                if (port.IsDeviceConnected)
                {
                    await RequestPortProperties(port);
                }
            }

            await _taskCompletionSource.Task;

            disposable.Dispose();
        }

        private async Task RequestPortProperties(PortInfo port)
        {
            lock (lockObject)
            {
                SentMessages += 2;
            }

            await _protocol.SendMessageAsync(new PortInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, InformationType = PortInformationType.ModeInfo, });
            await _protocol.SendMessageAsync(new PortInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, InformationType = PortInformationType.PossibleModeCombinations, });
        }

        private async Task RequestPortModePropertiesAsync(PortInfo port)
        {

            foreach (var modeIndex in port.Modes.Values.Select(m => m.ModeIndex))
            {
                lock (lockObject)
                {
                    SentMessages += 7;
                }

                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.Name, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.Raw, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.Pct, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.SI, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.Symbol, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.Mapping, });
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage() { HubId = _hubId, PortId = port.PortId, Mode = modeIndex, InformationType = PortModeInformationType.ValueFormat, });
            }

            // safeguard after awaits. first when all messages are sent out, completion can be reached.
            lock (lockObject)
            {
                _stageTwoCount++;
            }

            CheckEndOfDiscovery();
        }

        private void UpdateKnowledge(byte[] data, PoweredUpMessage message)
        {
            var knowledge = _protocol.Knowledge;

            var applicableMessage = KnowledgeManager.ApplyStaticProtocolKnowledge(message, knowledge);

            if (message is PortInformationForModeInfoMessage msg)
            {
                var port = knowledge.Port(_hubId, msg.PortId);

                RequestPortModePropertiesAsync(port);
            }

            if (applicableMessage)
            {
                ReceivedMessagesData.Add(data);

                lock (lockObject)
                {
                    ReceivedMessages++;
                }

                _logger?.LogInformation($"Stage: {_stageTwoCount}/{_stageTwoExpected}, Messages: {ReceivedMessages}/{SentMessages} ");

                CheckEndOfDiscovery();
            }
        }

        private void CheckEndOfDiscovery()
        {
            if (SentMessages == ReceivedMessages && _stageTwoCount >= _stageTwoExpected)
            {
                _taskCompletionSource.SetResult(ReceivedMessages);
            }
        }
    }
}