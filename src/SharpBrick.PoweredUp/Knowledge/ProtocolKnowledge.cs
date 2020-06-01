using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharpBrick.PoweredUp.Knowledge
{
    public class ProtocolKnowledge
    {
        public IDictionary<byte, PortInfo> Ports = new ConcurrentDictionary<byte, PortInfo>();

        public PortInfo Port(byte portId)
        {
            PortInfo result;
            if (!Ports.TryGetValue(portId, out result))
            {
                result = new PortInfo()
                {
                    PortId = portId,
                };

                Ports.Add(portId, result);
            }

            return result;
        }

        public PortModeInfo PortMode(byte portId, byte modeId)
        {
            var port = Port(portId);

            if (!(port.Modes.FirstOrDefault(m => m.ModeIndex == modeId) is var result))
            {
                throw new ArgumentException(nameof(modeId));
            }

            return result;
        }
    }
}