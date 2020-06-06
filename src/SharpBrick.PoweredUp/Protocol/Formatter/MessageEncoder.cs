using System;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class MessageEncoder
    {
        public static byte[] Encode(PoweredUpMessage message, ProtocolKnowledge knowledge)
        {
            var messageType = message switch
            {
                HubPropertyMessage msg => MessageType.HubProperties,
                HubActionMessage msg => MessageType.HubActions,
                HubAlertMessage msg => MessageType.HubAlerts,
                HubAttachedIOMessage msg => MessageType.HubAttachedIO,
                GenericErrorMessage msg => MessageType.GenericErrorMessages,

                PortInformationRequestMessage msg => MessageType.PortInformationRequest,
                PortModeInformationRequestMessage msg => MessageType.PortModeInformationRequest,
                PortInputFormatSetupSingleMessage msg => MessageType.PortInputFormatSetupSingle,
                PortInputFormatSetupCombinedModeMessage msg => MessageType.PortInputFormatSetupCombinedMode,
                PortInformationMessage msg => MessageType.PortInformation,
                PortModeInformationMessage msg => MessageType.PortModeInformation,
                PortValueSingleMessage msg => MessageType.PortValueSingle,
                PortValueCombinedModeMessage msg => MessageType.PortValueCombinedMode,
                PortInputFormatSingleMessage msg => MessageType.PortInputFormatSingle,
                PortInputFormatCombinedModeMessage msg => MessageType.PortInputFormatCombinedMode,
                VirtualPortSetupMessage msg => MessageType.VirtualPortSetup,
                PortOutputCommandMessage msg => MessageType.PortOutputCommand,
                _ => throw new NotImplementedException(),
            };

            var encoder = CreateEncoder(messageType, knowledge);

            var contentLength = encoder.CalculateContentLength(message);

            var commonHeaderLength = (contentLength + 3 > 127) ? 4 : 3;

            byte[] data = new byte[commonHeaderLength + contentLength];

            CommonMessageHeaderEncoder.Encode(contentLength, messageType, data.AsSpan().Slice(0, commonHeaderLength));
            encoder.Encode(message, data.AsSpan().Slice(commonHeaderLength));

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
                _ => null,
            };

        public static PoweredUpMessage Decode(in Span<byte> data, ProtocolKnowledge knowledge)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderEncoder.ParseCommonHeader(data);

            var encoder = CreateEncoder((MessageType)messageType, knowledge);

            var content = data.Slice(headerLength);

            PoweredUpMessage result;

            if (encoder != null)
            {
                var message = encoder?.Decode(content);

                message.Length = length;
                message.HubId = hubId;
                message.MessageType = (MessageType)messageType;

                result = message;
            }
            else
            {
                result = new UnknownMessage() { Data = data.ToArray() };
            }

            return result;
        }
    }
}