using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Devices;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModelHubBuilder
    {
        private readonly List<DeploymentDeviceModel> _devices = new();

        private SystemType? _hubType = null;

        public void AddHubType(SystemType? hubType)
            => _hubType = hubType;

        public DeploymentModelHubBuilder AddDevice<TDevice>(Port port)
            => AddDevice<TDevice>(port?.PortId ?? throw new ArgumentNullException(nameof(port)));

        public DeploymentModelHubBuilder AddDevice<TDevice>(byte portId)
            => AddDevice(DeviceFactory.GetDeviceTypeFromType(typeof(TDevice)), portId);

        public DeploymentModelHubBuilder AddAnyDevice(Port port)
            => AddAnyDevice(port.PortId);
        public DeploymentModelHubBuilder AddAnyDevice(byte portId)
            => AddDevice(null, portId);

        public DeploymentModelHubBuilder AddDevice(DeviceType? deviceType, byte portId)
        {
            _devices.Add(new DeploymentDeviceModel(portId, deviceType));

            return this;
        }

        public DeploymentHubModel Build()
            => new(_hubType, _devices.ToArray());
    }
}