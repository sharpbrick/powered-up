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

        Task SendMessageAsync(PoweredUpMessage message);
        IObservable<PoweredUpMessage> UpstreamMessages { get; }
        IObservable<(byte[] data, PoweredUpMessage message)> UpstreamRawMessages { get; }

        ProtocolKnowledge Knowledge { get; }

        IServiceProvider ServiceProvider { get; }
    }
}