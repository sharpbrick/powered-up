using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class MessageEncoder
    {
        public static byte[] Encode(PoweredUpMessage message)
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
                PortInputFormatSingleMessage msg => MessageType.PortInputFormatSingle,
                PortInputFormatCombinedModeMessage msg => MessageType.PortInputFormatCombinedMode,
                _ => throw new NotImplementedException(),
            };

            var encoder = CreateEncoder(messageType);

            var contentLength = encoder.CalculateContentLength(message);

            var commonHeaderLength = (contentLength + 3 > 127) ? 4 : 3;

            byte[] data = new byte[commonHeaderLength + contentLength];

            CommonMessageHeaderEncoder.Encode(contentLength, messageType, data.AsSpan().Slice(0, commonHeaderLength));
            encoder.Encode(message, data.AsSpan().Slice(commonHeaderLength));

            return data;
        }

        private static IMessageContentEncoder CreateEncoder(MessageType messageType)
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
                MessageType.PortInputFormatSingle => new PortInputFormatSingleEncoder(),
                MessageType.PortInputFormatCombinedMode => new PortInputFormatCombinedModeEncoder(),
                _ => null,
            };

        public static PoweredUpMessage Decode(in Span<byte> data)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderEncoder.ParseCommonHeader(data);

            var encoder = CreateEncoder((MessageType)messageType);

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
                result = new UnknownMessage();
            }

            return result;
        }
    }
}