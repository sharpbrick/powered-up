using System;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Functions
{
    public class DiscoverPorts
    {
        private readonly PoweredUpProtocol _protocol;
        private TaskCompletionSource<int> _taskCompletionSource;
        private int _stageTwoCount;

        public int SentMessages { get; private set; }
        public int ReceivedMessages { get; private set; }
        public DiscoverPorts(PoweredUpProtocol protocol)
        {
            _protocol = protocol ?? throw new System.ArgumentNullException(nameof(protocol));
        }

        public async Task ExecuteAsync()
        {
            _taskCompletionSource = new TaskCompletionSource<int>();
            _stageTwoCount = 0;

            SentMessages = 0;
            ReceivedMessages = 0;

            await _protocol.ReceiveMessageAsync(UpdateKnowledge);

            foreach (var port in _protocol.Knowledge.Ports.Values)
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

        private async Task UpdateKnowledge(PoweredUpMessage message)
        {
            var applicableMessage = true;
            var knowledge = _protocol.Knowledge;

            PortInfo port;
            PortModeInfo mode;

            switch (message)
            {
                case PortInformationForModeInfoMessage msg:
                    port = knowledge.Port(msg.PortId);

                    port.OutputCapability = msg.OutputCapability;
                    port.InputCapability = msg.InputCapability;
                    port.LogicalCombinableCapability = msg.LogicalCombinableCapability;
                    port.LogicalSynchronizableCapability = msg.LogicalSynchronizableCapability;
                    port.Modes = Enumerable.Range(0, msg.TotalModeCount).Select(modeIndex => new PortModeInfo()
                    {
                        PortId = msg.PortId,
                        ModeIndex = (byte)modeIndex,
                        IsInput = ((1 << modeIndex) & msg.InputModes) > 0,
                        IsOutput = ((1 << modeIndex) & msg.OutputModes) > 0
                    }).ToArray();

                    await RequestPortModeProperties(port);

                    break;
                case PortInformationForPossibleModeCombinationsMessage msg:
                    port = knowledge.Port(msg.PortId);

                    port.ModeCombinations = msg.ModeCombinations;
                    break;


                case PortModeInformationForNameMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.Name = msg.Name;
                    break;
                case PortModeInformationForRawMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.RawMin = msg.RawMin;
                    mode.RawMax = msg.RawMax;
                    break;
                case PortModeInformationForPctMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.PctMin = msg.PctMin;
                    mode.PctMax = msg.PctMax;
                    break;
                case PortModeInformationForSIMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.SIMin = msg.SIMin;
                    mode.SIMax = msg.SIMax;
                    break;
                case PortModeInformationForSymbolMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.Symbol = msg.Symbol;
                    break;
                case PortModeInformationForMappingMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.InputSupportsNull = msg.InputSupportsNull;
                    mode.InputSupportFunctionalMapping20 = msg.InputSupportFunctionalMapping20;
                    mode.InputAbsolute = msg.InputAbsolute;
                    mode.InputRelative = msg.InputRelative;
                    mode.InputDiscrete = msg.InputDiscrete;

                    mode.OutputSupportsNull = msg.OutputSupportsNull;
                    mode.OutputSupportFunctionalMapping20 = msg.OutputSupportFunctionalMapping20;
                    mode.OutputAbsolute = msg.OutputAbsolute;
                    mode.OutputRelative = msg.OutputRelative;
                    mode.OutputDiscrete = msg.OutputDiscrete;
                    break;
                case PortModeInformationForValueFormatMessage msg:
                    mode = knowledge.PortMode(msg.PortId, msg.Mode);

                    mode.NumberOfDatasets = msg.NumberOfDatasets;
                    mode.DatasetType = msg.DatasetType;
                    mode.TotalFigures = msg.TotalFigures;
                    mode.Decimals = msg.Decimals;
                    break;
                default:
                    applicableMessage = false;
                    break;
            }

            if (applicableMessage)
            {
                ReceivedMessages++;

                if (SentMessages == ReceivedMessages && _stageTwoCount >= knowledge.Ports.Count)
                {
                    _taskCompletionSource.SetResult(ReceivedMessages);
                }
            }
        }
    }
}