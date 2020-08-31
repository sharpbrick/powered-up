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
        private ILegoWirelessProtocol _protocol;
        private readonly ILogger<TraceMessages> _logger;

        public TraceMessages(ILegoWirelessProtocol protocol, ILogger<TraceMessages> logger = default)
        {
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _logger = logger;
        }

        public Task ExecuteAsync()
        {
            void TraceMessage(LegoWirelessMessage message)
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

        private static string MessageToString(LegoWirelessMessage message)
        {
            var result = message.ToString();

            if (result.Contains(message.GetType().Name))
            {
                result = $"{message.MessageType} in {message} (not yet formatted)";
            }

            return result;
        }
    }
}