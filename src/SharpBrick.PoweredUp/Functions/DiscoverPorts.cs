using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Functions
{
    public class DiscoverPorts
    {

        private readonly ILegoWirelessProtocol _protocol;
        private readonly byte _hubId;
        private readonly ILogger<DiscoverPorts> _logger;

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
            _logger?.LogInformation($"Number of Ports announced: {_protocol.Knowledge.Hub(_hubId).Ports.Count}");

            foreach (var port in _protocol.Knowledge.Hub(_hubId).Ports.Values.Where(p => portFilter == 0xFF || p.PortId == portFilter))
            {
                if (port.IsDeviceConnected)
                {
                    await RequestPortProperties(port);
                }
            }
        }

        private async Task RequestPortProperties(PortInfo port)
        {
            _logger?.LogInformation($"Discover Port {port.HubId}-{port.PortId}");
            var knowledge = _protocol.Knowledge;

            var portModeInfoMessage = await _protocol.SendMessageReceiveResultAsync<PortInformationForModeInfoMessage>(new PortInformationRequestMessage(port.PortId, PortInformationType.ModeInfo) { HubId = _hubId });
            var portModeCombinationsMessage = await _protocol.SendMessageReceiveResultAsync<PortInformationForPossibleModeCombinationsMessage>(new PortInformationRequestMessage(port.PortId, PortInformationType.PossibleModeCombinations) { HubId = _hubId });
            SentMessages += 2;
            ReceivedMessages += 2;

            KnowledgeManager.ApplyStaticProtocolKnowledge(portModeInfoMessage, knowledge);
            KnowledgeManager.ApplyStaticProtocolKnowledge(portModeCombinationsMessage, knowledge);

            await RequestPortModePropertiesAsync(port);
        }

        private async Task RequestPortModePropertiesAsync(PortInfo port)
        {
            var knowledge = _protocol.Knowledge;

            foreach (var modeIndex in port.Modes.Values.Select(m => m.ModeIndex))
            {
                _logger?.LogInformation($"Discover Mode {port.HubId}-{port.PortId}-{modeIndex}");

                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForNameMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Name) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForRawMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Raw) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForPctMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Pct) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForSIMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.SI) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForSymbolMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Symbol) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForMappingMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.Mapping) { HubId = _hubId }), knowledge);
                KnowledgeManager.ApplyStaticProtocolKnowledge(await _protocol.SendMessageReceiveResultAsync<PortModeInformationForValueFormatMessage>(new PortModeInformationRequestMessage(port.PortId, modeIndex, PortModeInformationType.ValueFormat) { HubId = _hubId }), knowledge);

                SentMessages += 7;
                ReceivedMessages += 7;
            }
        }
    }
}