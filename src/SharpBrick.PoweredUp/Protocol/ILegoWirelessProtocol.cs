using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol;

public interface ILegoWirelessProtocol : IDisposable
{
    Task ConnectAsync(SystemType initialSystemType = default);
    Task DisconnectAsync();

    Task SendMessageAsync(LegoWirelessMessage message);
    IObservable<LegoWirelessMessage> UpstreamMessages { get; }
    IObservable<(byte[] data, LegoWirelessMessage message)> UpstreamRawMessages { get; }

    ProtocolKnowledge Knowledge { get; }

    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Indicates if this is running in normal mode, using known configurations/behavior of hub and devices (default). 
    /// Or in discovery mode where all hub and devices are queried for their knowledge and any known configuration is ignored.
    /// </summary>
    bool DiscoveryMode { get; set; }
}
