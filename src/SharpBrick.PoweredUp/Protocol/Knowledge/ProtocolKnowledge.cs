using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharpBrick.PoweredUp.Protocol.Knowledge
{
    public class ProtocolKnowledge
    {
        private ConcurrentDictionary<byte, HubInfo> _hubs = new ConcurrentDictionary<byte, HubInfo>();
        public IEnumerable<HubInfo> Hubs => _hubs.Values;

        public HubInfo Hub(byte hubId)
        {
            HubInfo result;

            if (!_hubs.TryGetValue(hubId, out result))
            {
                result = new HubInfo()
                {
                    HubId = hubId,
                };

                _hubs.TryAdd(hubId, result);
            }

            return result;
        }


        public PortInfo Port(byte hubId, byte portId)
        {
            var hub = Hub(hubId);

            PortInfo result;
            if (!hub.Ports.TryGetValue(portId, out result))
            {
                result = new PortInfo()
                {
                    HubId = hub.HubId,
                    PortId = portId,
                };

                hub.Ports.TryAdd(portId, result);
            }

            return result;
        }

        public PortModeInfo PortMode(byte hubId, byte portId, byte modeIndex)
        {
            var port = Port(hubId, portId);

            if (!(port.Modes.TryGetValue(modeIndex, out var result)))
            {
                throw new ArgumentException(nameof(modeIndex));
            }

            return result;
        }
    }
}