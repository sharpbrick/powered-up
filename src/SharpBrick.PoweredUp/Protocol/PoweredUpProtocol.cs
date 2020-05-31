using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public class PoweredUpProtocol
    {
        private readonly BluetoothKernel _kernel;

        public PoweredUpProtocol(BluetoothKernel kernel)
        {
            _kernel = kernel;
        }
        public async Task SendMessageAsync(PoweredUpMessage message)
        {
            var data = MessageEncoder.Encode(message);

            await _kernel.SendBytesAsync(data);
        }

        public async Task ReceiveMessageAsync(Func<PoweredUpMessage, Task> handler)
        {
            await _kernel.ReceiveBytesAsync(async data =>
            {
                var message = MessageEncoder.Decode(data);

                await handler(message);
            });
        }
    }
}