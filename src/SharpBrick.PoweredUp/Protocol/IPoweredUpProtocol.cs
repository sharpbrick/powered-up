using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Protocol
{
    public interface IPoweredUpProtocol
    {
        Task SendMessageAsync(PoweredUpMessage message);
        IObservable<PoweredUpMessage> UpstreamMessages { get; }
    }
}