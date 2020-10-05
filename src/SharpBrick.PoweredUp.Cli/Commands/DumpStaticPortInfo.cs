using System;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Cli
{
    public class DumpStaticPortInfo
    {
        private readonly ILegoWirelessProtocol protocol;
        private readonly DiscoverPorts discoverPorts;

        public DumpStaticPortInfo(ILegoWirelessProtocol protocol, DiscoverPorts discoverPorts)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            this.discoverPorts = discoverPorts ?? throw new ArgumentNullException(nameof(discoverPorts));
        }
        public async Task ExecuteAsync(SystemType knownSystemType, byte portId)
        {
            Console.WriteLine($"Discover Port {portId}. Receiving Messages ...");

            await protocol.ConnectAsync(knownSystemType); // registering to bluetooth notification

            await Task.Delay(2000); // await ports to be announced initially by device.

            using var disposable = protocol.UpstreamMessages.Subscribe(x => Console.Write("."));

            await discoverPorts.ExecuteAsync(portId);

            await protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage() { HubId = 0, Action = HubAction.SwitchOffHub }, result => result.Action == HubAction.HubWillSwitchOff);

            Console.WriteLine(string.Empty);

            Console.WriteLine($"Discover Ports Function: {discoverPorts.ReceivedMessages} / {discoverPorts.SentMessages}");

            Console.WriteLine("##################################################");
            foreach (var data in discoverPorts.ReceivedMessagesData.OrderBy(x => x[2]).ThenBy(x => x[4]).ThenBy(x => (x.Length <= 5) ? -1 : x[5]))
            {
                Console.WriteLine(BytesStringUtil.DataToString(data));
            }
            Console.WriteLine("##################################################");
        }
    }
}
