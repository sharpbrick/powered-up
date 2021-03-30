using System;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class MessageEncoder
    {
        public static byte[] Encode(LegoWirelessMessage message, ProtocolKnowledge knowledge)
        {
            var messageType = message switch
            {
                HubPropertyMessage => MessageType.HubProperties,
                HubActionMessage => MessageType.HubActions,
                HubAlertMessage => MessageType.HubAlerts,
                HubAttachedIOMessage => MessageType.HubAttachedIO,
                GenericErrorMessage => MessageType.GenericErrorMessages,

                PortInformationRequestMessage => MessageType.PortInformationRequest,
                PortModeInformationRequestMessage => MessageType.PortModeInformationRequest,
                PortInputFormatSetupSingleMessage => MessageType.PortInputFormatSetupSingle,
                PortInputFormatSetupCombinedModeMessage => MessageType.PortInputFormatSetupCombinedMode,
                PortInformationMessage => MessageType.PortInformation,
                PortModeInformationMessage => MessageType.PortModeInformation,
                PortValueSingleMessage => MessageType.PortValueSingle,
                PortValueCombinedModeMessage => MessageType.PortValueCombinedMode,
                PortInputFormatSingleMessage => MessageType.PortInputFormatSingle,
                PortInputFormatCombinedModeMessage => MessageType.PortInputFormatCombinedMode,
                VirtualPortSetupMessage => MessageType.VirtualPortSetup,
                PortOutputCommandMessage => MessageType.PortOutputCommand,
                PortOutputCommandFeedbackMessage => MessageType.PortOutputCommandFeedback,
                _ => throw new NotImplementedException(),
            };

            var encoder = CreateEncoder(messageType, knowledge);

            var contentLength = encoder.CalculateContentLength(message);

            var commonHeaderLength = (contentLength + 3 > 127) ? 4 : 3;

            byte[] data = new byte[commonHeaderLength + contentLength];

            CommonMessageHeaderEncoder.Encode(contentLength, message.HubId, messageType, data.AsSpan().Slice(0, commonHeaderLength));
            encoder.Encode(message, data.AsSpan()[commonHeaderLength..]);

            return data;
        }

        private static IMessageContentEncoder CreateEncoder(MessageType messageType, ProtocolKnowledge knowledge)
            => messageType switch
            {
                MessageType.HubProperties => new HubPropertiesEncoder(),
                MessageType.HubActions => new HubActionEncoder(),
                MessageType.HubAlerts => new HubAlertEncoder(),
                MessageType.HubAttachedIO => new HubAttachedIOEncoder(),
                MessageType.GenericErrorMessages => new GenericErrorMessageEncoder(),

                MessageType.PortInformationRequest => new PortInformationRequestEncoder(),
                MessageType.PortModeInformationRequest => new PortModeInformationRequestEncoder(),
                MessageType.PortInputFormatSetupSingle => new PortInputFormatSetupSingleEncoder(),
                MessageType.PortInputFormatSetupCombinedMode => new PortInputFormatSetupCombinedModeEncoder(),
                MessageType.PortInformation => new PortInformationEncoder(),
                MessageType.PortModeInformation => new PortModeInformationEncoder(),
                MessageType.PortValueSingle => new PortValueSingleEncoder(knowledge),
                MessageType.PortValueCombinedMode => new PortValueCombinedModeEncoder(knowledge),
                MessageType.PortInputFormatSingle => new PortInputFormatSingleEncoder(),
                MessageType.PortInputFormatCombinedMode => new PortInputFormatCombinedModeEncoder(),
                MessageType.VirtualPortSetup => new VirtualPortSetupEncoder(),
                MessageType.PortOutputCommand => new PortOutputCommandEncoder(),
                MessageType.PortOutputCommandFeedback => new PortOutputCommandFeedbackEncoder(),
                _ => null,
            };

        public static LegoWirelessMessage Decode(in Span<byte> data, ProtocolKnowledge knowledge)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderEncoder.ParseCommonHeader(data);

            var encoder = CreateEncoder((MessageType)messageType, knowledge);

            var content = data[headerLength..];

            LegoWirelessMessage result;

            if (encoder is not null)
            {
                var message = encoder?.Decode(hubId, content);

                message.Length = length;
                message.HubId = hubId;

                if (message.MessageType != (MessageType)messageType)
                {
                    throw new InvalidOperationException("type in data does not match message type");
                }

                result = message;
            }
            else
            {
                result = new UnknownMessage(messageType, data.ToArray());
            }

            return result;
        }
    }
}