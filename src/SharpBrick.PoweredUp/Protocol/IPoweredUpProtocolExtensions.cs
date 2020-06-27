using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public static class IPoweredUpProtocolExtensions
    {
        public static async Task<TResultMessage> SendMessageReceiveResultAsync<TResultMessage>(this IPoweredUpProtocol self, PoweredUpMessage message, Func<TResultMessage, bool> filter = default)
        {
            var awaitable = self.UpstreamMessages
                .OfType<TResultMessage>()
                .Where(resultMessage => filter == null || filter(resultMessage))
                .FirstAsync()
                .GetAwaiter(); // make sure the subscription is present at the moment the message is sent.

            await self.SendMessageAsync(message);

            var result = await awaitable;

            return result;
        }

        public static async Task<PortFeedback> SendPortOutputCommandAsync(this IPoweredUpProtocol self, PortOutputCommandMessage message)
        {
            var portId = message.PortId;

            var response = await self.SendMessageReceiveResultAsync<PortOutputCommandFeedbackMessage>(message, msg => msg.Feedbacks.Any(f => f.PortId == portId));

            return response.Feedbacks.FirstOrDefault(f => f.PortId == portId).Feedback;
        }
    }
}