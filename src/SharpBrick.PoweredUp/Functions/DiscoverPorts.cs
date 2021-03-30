using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Functions
{
    public class DiscoverPorts
    {
        private readonly object lockObject = new();

        private readonly ILegoWirelessProtocol _protocol;
        private readonly byte _hubId;
        private readonly ILogger<DiscoverPorts> _logger;
        private TaskCompletionSource<int> _taskCompletionSource;
        private int _stageTwoCount;
        private int _stageTwoExpected;
        private SemaphoreSlim _semaphore;

        public int SentMessages { get; private set; }
        public int ReceivedMessages { get; private set; }

        public ConcurrentBag<byte[]> ReceivedMessagesData { get; private set; }

        public DiscoverPorts(ILegoWirelessProtocol protocol, byte hubId = 0, ILogger<DiscoverPorts> logger = default)
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
            //on Non-WinRT-bluetooth-devices we will get a race-condition, because too many writes get
            //to the Hub and some of them will not be answered at all;
            //so it's best to make it here "sequential" by using a semaphore which is released on every UpdateKnowledge 
            _semaphore = new SemaphoreSlim(0, 1);

            using var disposable = _protocol.UpstreamRawMessages.Subscribe(tuple => UpdateKnowledge(tuple.data, tuple.message));

            _stageTwoExpected = _protocol.Knowledge.Hub(_hubId).Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter).Count();

            _logger?.LogInformation($"Number of Ports: {_stageTwoExpected}");
            _semaphore.Release(1);

            foreach (var port in _protocol.Knowledge.Hub(_hubId).Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter))
            {
                if (port.IsDeviceConnected)
                {
                    await RequestPortProperties(port);
                }
            }
            //in case no device is connected to the Hub, the previuos foreach will be skipped and the task _taskCompletionSource will not be returned
            //so call here CheckEndOfDiscovery
            CheckEndOfDiscovery();
            await _taskCompletionSource.Task;

            disposable.Dispose();
        }

        private async Task RequestPortProperties(PortInfo port)
        {
            lock (lockObject)
            {
                SentMessages += 2;
            }
            //reallay important to use async form of wait here, because otherwise the UpdateKnowledge-calls will be blocked
            await _semaphore.WaitAsync();
            await _protocol.SendMessageAsync(new PortInformationRequestMessage(port.PortId, PortInformationType.ModeInfo) { HubId = _hubId });
            await _semaphore.WaitAsync();
            await _protocol.SendMessageAsync(new PortInformationRequestMessage(port.PortId, PortInformationType.PossibleModeCombinations) { HubId = _hubId });
        }

        private async Task RequestPortModePropertiesAsync(PortInfo port)
        {

            foreach (var modeIndex in port.Modes.Values.Select(m => m.ModeIndex))
            {
                lock (lockObject)
                {
                    SentMessages += 7;
                }
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Name) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Raw) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Pct) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.SI) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Symbol) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Mapping) { HubId = _hubId });
                await _semaphore.WaitAsync();
                await _protocol.SendMessageAsync(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.ValueFormat) { HubId = _hubId });
            }

            // safeguard after awaits. first when all messages are sent out, completion can be reached.
            lock (lockObject)
            {
                _stageTwoCount++;
            }

            CheckEndOfDiscovery();
        }

        private void UpdateKnowledge(byte[] data, LegoWirelessMessage message)
        {
            
            var knowledge = _protocol.Knowledge;

            var applicableMessage = KnowledgeManager.ApplyStaticProtocolKnowledge(message, knowledge);

            if (message is PortInformationForModeInfoMessage msg)
            {
                var port = knowledge.Port(_hubId, msg.PortId);

                _ = RequestPortModePropertiesAsync(port); // discard the task to supress the await error
            }

            if (applicableMessage)
            {
                ReceivedMessagesData.Add(data);

                lock (lockObject)
                {
                    ReceivedMessages++;
                    _ = _semaphore.Release();
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