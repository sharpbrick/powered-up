using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Functions;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public class DynamicDevice : Device, IPoweredUpDevice
    {
        public DynamicDevice()
        { }

        public DynamicDevice(IPoweredUpProtocol protocol, byte hubId, byte portId)
            : base(protocol, hubId, portId)
        { }

        public async Task DiscoverAsync()
        {
            var discoverPortsFunction = new DiscoverPorts(_protocol);

            await discoverPortsFunction.ExecuteAsync(_portId);

            BuildModes();
        }

        public IEnumerable<byte[]> GetStaticPortInfoMessages(Version softwareVersion, Version hardwareVersion)
            => Array.Empty<byte[]>();
    }
}