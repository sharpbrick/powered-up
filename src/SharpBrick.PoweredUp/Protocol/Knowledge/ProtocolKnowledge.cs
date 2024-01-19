using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SharpBrick.PoweredUp.Protocol.Knowledge;

public class ProtocolKnowledge
{
    private readonly ConcurrentDictionary<byte, HubInfo> _hubs = new();
    public IEnumerable<HubInfo> Hubs => _hubs.Values;

    public HubInfo Hub(byte hubId)
    {
        if (!_hubs.TryGetValue(hubId, out HubInfo result))
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

        if (!hub.Ports.TryGetValue(portId, out PortInfo result))
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
            throw new ArgumentException($"Unknown modeIndex {modeIndex}, on portId {portId} for hubId {hubId}", nameof(modeIndex));
        }

        return result;
    }
}
