using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Functions
{
    public class TraceMessages
    {
        private IPoweredUpProtocol _protocol;
        private readonly ILogger<TraceMessages> _logger;

        public TraceMessages(IPoweredUpProtocol protocol, ILogger<TraceMessages> logger = default)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _logger = logger;
        }

        public Task ExecuteAsync()
        {
            void TraceMessage(PoweredUpMessage message)
            {
                try
                {
                    var messageAsString = MessageToString(message);

                    if (message is GenericErrorMessage)
                    {
                        _logger.LogError(messageAsString);
                    }
                    else
                    {
                        _logger.LogInformation(messageAsString);
                    }
                }
                catch (Exception e) // swallow. it is just trace
                {
                    _logger.LogCritical(e, "Exception trace generation");
                }
            }

            _protocol.UpstreamMessages.Subscribe(TraceMessage);

            return Task.CompletedTask;
        }

        private static string MessageToString(PoweredUpMessage message)
        {
            string messageAsString = message switch
            {
                HubPropertyMessage<Version> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                HubPropertyMessage<string> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                HubPropertyMessage<bool> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                HubPropertyMessage<sbyte> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                HubPropertyMessage<byte> msg => $"Hub Property - {msg.Property}: {msg.Payload}",
                HubPropertyMessage<byte[]> msg => $"Hub Property - {msg.Property}: {BytesStringUtil.DataToString(msg.Payload)}",
                HubActionMessage msg => $"Hub Action - {msg.Action}",
                HubAttachedIOForAttachedDeviceMessage msg => $"Attached IO - Port {msg.PortId} of type {msg.IOTypeId} (HW: {msg.HardwareRevision} / SW: {msg.SoftwareRevision})",
                HubAttachedIOForAttachedVirtualDeviceMessage msg => $"Attached Virtual IO - Port {msg.PortId} with A {msg.PortAId} / B {msg.PortBId}  of type {msg.IOTypeId}",
                HubAttachedIOForDetachedDeviceMessage msg => $"Dettached IO - Port {msg.PortId}",
                GenericErrorMessage msg => $"Error - {msg.ErrorCode} from {(MessageType)msg.CommandType}",
                PortInformationForModeInfoMessage msg => $"Port Information - Port {msg.PortId} Total Modes {msg.TotalModeCount} / Capabilities Output:{msg.OutputCapability}, Input:{msg.InputCapability}, LogicalCombinable:{msg.LogicalCombinableCapability}, LogicalSynchronizable:{msg.LogicalSynchronizableCapability} / InputModes: {msg.InputModes:X}, OutputModes: {msg.InputModes:X}",
                PortInformationForPossibleModeCombinationsMessage msg => $"Port Information (Combinations) - Port {msg.PortId} Combinations: {string.Join(",", msg.ModeCombinations.Select(x => x.ToString("X")))}",
                PortValueSingleMessage msg => "Port Values - " + string.Join(";", msg.Data.Select(d => d switch
                {
                    PortValueData<sbyte> dd => $"Port {dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<short> dd => $"Port {dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<int> dd => $"Port {dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<float> dd => $"Port {dd.PortId}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    _ => "Undefined Data Type",
                })),
                PortValueCombinedModeMessage msg => $"Port Value (Combined Mode) - Port {msg.PortId} " + string.Join(";", msg.Data.Select(d => d switch
                {
                    PortValueData<sbyte> dd => $"Mode {dd.ModeIndex}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<short> dd => $"Mode {dd.ModeIndex}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<int> dd => $"Mode {dd.ModeIndex}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    PortValueData<float> dd => $"Mode {dd.ModeIndex}/{dd.ModeIndex}: {string.Join(",", dd.InputValues)} ({dd.DataType})",
                    _ => "Undefined Data Type",
                })),
                PortInputFormatSingleMessage msg => $"Port Input Format (Single) - Port {msg.PortId}, Mode {msg.Mode}, Threshold {msg.DeltaInterval}, Notification {msg.NotificationEnabled}",
                PortInputFormatCombinedModeMessage msg => $"Port Input Format (Combined Mode) - Port {msg.PortId} UsedCombinationIndex {msg.UsedCombinationIndex} Enabled {msg.MultiUpdateEnabled} Configured Modes {string.Join(",", msg.ConfiguredModeDataSetIndex)}",
                PortOutputCommandFeedbackMessage msg => $"Port Command Feedback - " + string.Join(",", msg.Feedbacks.Select(f => $"Port {f.PortId} -> {f.Feedback}")),
                UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {BytesStringUtil.DataToString(msg.Data)}",
                var unknown => $"{unknown.MessageType} (not yet formatted)",
            };

            return messageAsString;
        }
    }
}