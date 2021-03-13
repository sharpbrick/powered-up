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

            if (systemType != 0)
            {
                Console.Error.WriteLine("Command Line provided Device Type, hubId, portId and versions hw/sw");
                await _mock.MockCharacteristic.AttachIO((DeviceType)deviceType, hubId, portId, hw, sw);
            }

            var foundSystemType = false;
            var foundAttachedIO = false;

            var line = await reader.ReadLineAsync();
            while (line is not null)
            {
                if (line.Substring(6, 8) == "01-0B-06") // property msg - systemtype - update
                {
                    foundSystemType = true;
                }
                if (line.Substring(6, 2) == "04" && line.Substring(12, 2) == "01") // attached io msg (04) - port - attach event
                {
                    foundAttachedIO = true;
                }
                await _mock.MockCharacteristic.WriteUpstreamAsync(line);

                line = await reader.ReadLineAsync();
            }

            if (systemType == 0 && (foundSystemType == false || foundAttachedIO == false))
            {
                Console.Error.WriteLine("#############################");
                Console.Error.WriteLine("SystemType and/or attached IO message not found in data or command line arguments");
                Console.Error.WriteLine("#############################");
            }

            DevicesList.PrettyPrintKnowledge(System.Console.Out, _protocol.Knowledge);
        }
    }
}