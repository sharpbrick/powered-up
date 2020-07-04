using System;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentHubModel
    {
        public SystemType HubType { get; }
        public DeploymentDeviceModel[] Devices { get; }

        public DeploymentHubModel(SystemType hubType, DeploymentDeviceModel[] devices)
        {
            HubType = hubType;
            Devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }
    }
}