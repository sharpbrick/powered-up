using System;
using System.IO;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Bluetooth.Mock;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Protocol.Knowledge;

namespace SharpBrick.PoweredUp.Cli
{
    public class PrettyPrint
    {
        private readonly ILegoWirelessProtocol _protocol;
        private readonly PoweredUpBluetoothAdapterMock _mock;

        public PrettyPrint(ILegoWirelessProtocol protocol, IServiceProvider serviceProvider)
        {
            this._protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this._mock = serviceProvider.GetMockBluetoothAdapter();
        }

        public async Task ExecuteAsync(TextReader reader, byte systemType, ushort deviceType, byte hubId, byte portId, Version hw, Version sw)
        {
            var knowledge = _protocol.Knowledge;

            // override cached data
            using var disposable = _protocol.UpstreamRawMessages
                .Subscribe(tuple => KnowledgeManager.ApplyStaticProtocolKnowledge(tuple.message, knowledge));

            await _protocol.ConnectAsync((SystemType)systemType); // registering to bluetooth notification

            await _mock.MockCharacteristic.AttachIO((DeviceType)deviceType, hubId, portId, hw, sw);

            var line = await reader.ReadLineAsync();
            while (line != null)
            {
                await _mock.MockCharacteristic.WriteUpstreamAsync(line);

                line = await reader.ReadLineAsync();
            }

            DevicesList.PrettyPrintKnowledge(System.Console.Out, _protocol.Knowledge);
        }
    }
}