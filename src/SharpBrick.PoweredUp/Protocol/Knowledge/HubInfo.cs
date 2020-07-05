using System.Collections.Concurrent;

namespace SharpBrick.PoweredUp.Protocol.Knowledge
{
    public class HubInfo
    {
        public byte HubId { get; set; }

        public SystemType SystemType { get; set; }

        public ConcurrentDictionary<byte, PortInfo> Ports { get; set; } = new ConcurrentDictionary<byte, PortInfo>();
    }
}