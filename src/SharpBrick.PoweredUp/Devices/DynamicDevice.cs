using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp
{
    public class DynamicDevice : Device, IPoweredUpDevice
    {
        public DynamicDevice()
        { }

        public DynamicDevice(ILegoWirelessProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public async Task DiscoverAsync()
        {
            var discoverPortsFunction = new DiscoverPorts(_protocol);

            await discoverPortsFunction.ExecuteAsync(_portId);

            BuildModes();
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion, SystemType systemType)
            => Array.Empty<byte[]>();
    }
}