namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentDeviceModel
    {
        /// <summary>
        /// Expected PortId
        /// </summary>
        /// <value></value>
        public byte PortId { get; }

        /// <summary>
        /// Expected Device Type
        /// </summary>
        /// <value></value>
        public DeviceType? DeviceType { get; }

        public DeploymentDeviceModel(byte portId, DeviceType? deviceType)
        {
            PortId = portId;
            DeviceType = deviceType;
        }
    }
}