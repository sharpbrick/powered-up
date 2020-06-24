using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public class PoweredUpProtocol : IPoweredUpProtocol
    {
        private readonly BluetoothKernel _kernel;
        private readonly ILogger<PoweredUpProtocol> _logger;
        private Subject<(byte[] data, PoweredUpMessage message)> _upstreamSubject = null;

        public ProtocolKnowledge Knowledge { get; } = new ProtocolKnowledge();

        public IObservable<(byte[] data, PoweredUpMessage message)> UpstreamRawMessages => _upstreamSubject;
        public IObservable<PoweredUpMessage> UpstreamMessages => _upstreamSubject.Select(x => x.message);

        public PoweredUpProtocol(BluetoothKernel kernel, ILogger<PoweredUpProtocol> logger = default)
        {
            _kernel = kernel;
            _logger = logger;
            _upstreamSubject = new Subject<(byte[] data, PoweredUpMessage message)>();
        }

        public async Task ConnectAsync()
        {
            await _kernel.ReceiveBytesAsync(async data =>
            {
                try
                {
                    var message = MessageEncoder.Decode(data, Knowledge);

                    await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, Knowledge);

                    _upstreamSubject.OnNext((data, message));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in PoweredUpProtocol Decode/Knowledge");

                    throw;
                }
            });
        }

        public async Task SendMessageAsync(PoweredUpMessage message)
        {
            try
            {
                var data = MessageEncoder.Encode(message, Knowledge);

                await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, Knowledge);

                await _kernel.SendBytesAsync(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in PoweredUpProtocol Encode/Knowledge");

                throw;
            }
        }
    }
}