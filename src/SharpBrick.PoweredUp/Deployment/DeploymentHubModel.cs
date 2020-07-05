using System;

namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentHubModel
    {
        /// <summary>
        /// Expected Hub Type.
        /// </summary>
        /// <value></value>
        public SystemType? HubType { get; }

        /// <summary>
        /// List of expected devices
        /// </summary>
        /// <value></value>
        public DeploymentDeviceModel[] Devices { get; }

        public DeploymentHubModel(SystemType? hubType, DeploymentDeviceModel[] devices)
        {
            HubType = hubType;
            Devices = devices ?? throw new ArgumentNullException(nameof(devices));
        }
    }
}