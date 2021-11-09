using System;
using System.Linq;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Cli;

public class DumpStaticPortInfo
{
    private readonly ILegoWirelessProtocol protocol;
    private readonly DiscoverPorts discoverPorts;

    public DumpStaticPortInfo(ILegoWirelessProtocol protocol, DiscoverPorts discoverPorts)
    {
        this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        this.discoverPorts = discoverPorts ?? throw new ArgumentNullException(nameof(discoverPorts));
    }
    public async Task ExecuteAsync(SystemType knownSystemType, byte portId, bool headerEnabled)
    {
        Console.WriteLine($"Discover Port {portId}. Receiving Messages ...");

        await protocol.ConnectAsync(knownSystemType); // registering to bluetooth notification

        await Task.Delay(2000); // await ports to be announced initially by device.

        using var disposable = protocol.UpstreamMessages.Subscribe(x => Console.Write("."));

        await discoverPorts.ExecuteAsync(portId);

        await protocol.SendMessageReceiveResultAsync<HubActionMessage>(new HubActionMessage(HubAction.SwitchOffHub) { HubId = 0 }, result => result.Action == HubAction.HubWillSwitchOff);

        Console.WriteLine(string.Empty);

        Console.WriteLine($"Discover Ports Function: {discoverPorts.ReceivedMessages} / {discoverPorts.SentMessages}");

        Console.WriteLine(knownSystemType);
        Console.WriteLine("##################################################");
        //Exception was thrown if no device was attached to the port
        if (discoverPorts.ReceivedMessages > 0)
        {
            var systemTypeMessage = CreateSystemTypeHeader(knownSystemType);
            var attachedIOMessage = CreateAttachedIOHeader(portId);


            if (headerEnabled)
            {
                Console.WriteLine(BytesStringUtil.DataToString(systemTypeMessage));
                Console.WriteLine(BytesStringUtil.DataToString(attachedIOMessage));
            }

            foreach (var data in discoverPorts.ReceivedMessagesData.OrderBy(x => x[2]).ThenBy(x => x[4]).ThenBy(x => (x.Length <= 5) ? -1 : x[5]))
            {
                Console.WriteLine(BytesStringUtil.DataToString(data));
            }
        }
        else
        {
            Console.WriteLine($":-( It seems there is no device attached to port {portId} on this hub...");
        }

        Console.WriteLine("##################################################");
    }

    private byte[] CreateAttachedIOHeader(byte portId)
    {
        var portInfo = protocol.Knowledge.Port(0, portId);

        return MessageEncoder.Encode(portInfo switch
        {
            { IsVirtual: false } => new HubAttachedIOForAttachedDeviceMessage(portId, portInfo.IOTypeId, portInfo.HardwareRevision, portInfo.SoftwareRevision)
            {
                HubId = 0,
            },
            { IsVirtual: true } => new HubAttachedIOForAttachedVirtualDeviceMessage(portId, portInfo.IOTypeId, portInfo.PortAId, portInfo.PortBId)
            {
                HubId = 0,
            }
        }, protocol.Knowledge);
    }

    private byte[] CreateSystemTypeHeader(SystemType knownSystemType)
        => MessageEncoder.Encode(new HubPropertyMessage<SystemType>(HubProperty.SystemTypeId, HubPropertyOperation.Update, knownSystemType)
        {
            HubId = 0,
        }, protocol.Knowledge);
}
