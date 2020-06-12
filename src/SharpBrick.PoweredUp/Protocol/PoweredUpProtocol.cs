using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Knowledge;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public class PoweredUpProtocol : IPoweredUpProtocol
    {
        private readonly BluetoothKernel _kernel;
        private readonly ILogger<PoweredUpProtocol> _logger;
        private Subject<PoweredUpMessage> _upstreamSubject = null;

        public ProtocolKnowledge Knowledge = new ProtocolKnowledge();

        public IObservable<PoweredUpMessage> UpstreamMessages => _upstreamSubject;

        public PoweredUpProtocol(BluetoothKernel kernel, ILogger<PoweredUpProtocol> logger = default)
        {
            _kernel = kernel;
            _logger = logger;
            _upstreamSubject = new Subject<PoweredUpMessage>();
        }

        public async Task SetupUpstreamObservableAsync()
        {
            await ReceiveMessageAsync(message =>
            {
                _upstreamSubject.OnNext(message);

                return Task.CompletedTask;
            });
        }

        public async Task SendMessageAsync(PoweredUpMessage message)
        {
            try
            {
                var knowledge = Knowledge;

                var data = MessageEncoder.Encode(message, knowledge);

                await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, knowledge);

                await _kernel.SendBytesAsync(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in PoweredUpProtocol Encode/Knowledge");

                throw;
            }
        }

        public Task ReceiveMessageAsync(Func<PoweredUpMessage, Task> handler)
            => ReceiveMessageAsync((data, message) => handler(message));
        public async Task ReceiveMessageAsync(Func<byte[], PoweredUpMessage, Task> handler)
        {
            await _kernel.ReceiveBytesAsync(async data =>
            {
                try
                {
                    var knowledge = Knowledge;

                    var message = MessageEncoder.Decode(data, knowledge);

                    await KnowledgeManager.ApplyDynamicProtocolKnowledge(message, Knowledge);

                    await handler(data, message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in PoweredUpProtocol Decode/Knowledge");

                    throw;
                }
            });
        }
    }
}