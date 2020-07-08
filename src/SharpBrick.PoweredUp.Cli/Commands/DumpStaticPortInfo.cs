using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using SharpBrick.PoweredUp.WinRT;

namespace SharpBrick.PoweredUp.Cli
{
    public static class DumpStaticPortInfo
    {
        public static async Task ExecuteAsync(ILoggerFactory loggerFactory, WinRTPoweredUpBluetoothAdapter poweredUpBluetoothAdapter, ulong bluetoothAddress, byte portId, bool enableTrace)
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var protocol = new PoweredUpProtocol(
                new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()),
                serviceProvider))
            {
                var discoverPorts = new DiscoverPorts(protocol, logger: loggerFactory.CreateLogger<DiscoverPorts>());

                if (enableTrace)
                {
                    var tracer = new TraceMessages(protocol, loggerFactory.CreateLogger<TraceMessages>());

                    await tracer.ExecuteAsync();
                }

                Console.WriteLine($"Discover Port {portId}. Receiving Messages ...");

                await protocol.ConnectAsync(); // registering to bluetooth notification

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
}
