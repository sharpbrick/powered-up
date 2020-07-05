namespace SharpBrick.PoweredUp.Deployment
{
    public class DeploymentDeviceModel
    {
        public byte PortId { get; }
        public DeviceType? DeviceType { get; }

        public DeploymentDeviceModel(byte portId, DeviceType? deviceType)
        {
            PortId = portId;
            DeviceType = deviceType;
        }
    }
}