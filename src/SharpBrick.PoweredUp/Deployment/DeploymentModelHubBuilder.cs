using System;
using System.Collections.Generic;
using SharpBrick.PoweredUp.Devices;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentModelHubBuilder<THub> where THub : Hub
    {
        private List<DeploymentDeviceModel> _devices = new List<DeploymentDeviceModel>();
        internal IEnumerable<DeploymentDeviceModel> Devices => _devices;


        public DeploymentModelHubBuilder<THub> AddDevice<TDevice>(Port port)
            => AddDevice<TDevice>(port?.PortId ?? throw new ArgumentNullException(nameof(port)));

        public DeploymentModelHubBuilder<THub> AddDevice<TDevice>(byte portId)
            => AddDevice(DeviceFactory.GetDeviceTypeFromType(typeof(TDevice)), portId);

        public DeploymentModelHubBuilder<THub> AddDevice(DeviceType deviceType, byte portId)
        {
            _devices.Add(new DeploymentDeviceModel(portId, deviceType));

            return this;
        }
    }
}