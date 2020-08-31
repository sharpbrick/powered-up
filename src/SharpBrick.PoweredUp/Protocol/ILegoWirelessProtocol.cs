using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Knowledge;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public interface ILegoWirelessProtocol : IDisposable
    {
        Task ConnectAsync();
        Task DisconnectAsync();

        Task SendMessageAsync(LegoWirelessMessage message);
        IObservable<LegoWirelessMessage> UpstreamMessages { get; }
        IObservable<(byte[] data, LegoWirelessMessage message)> UpstreamRawMessages { get; }

        ProtocolKnowledge Knowledge { get; }

        IServiceProvider ServiceProvider { get; }
    }
}