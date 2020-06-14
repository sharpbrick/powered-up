using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp
{
    public class TechnicMediumHub : Hub
    {
        public TechnicMediumHub(ILogger logger = default)
            : base(logger, new Port[] {
                new Port(0, "A", true),
                new Port(1, "B", true),
                new Port(2, "C", true),
                new Port(3, "D", true),
                new Port(50, string.Empty, false, expectedDevice: DeviceType.RgbLight),
                new Port(59, string.Empty, false, expectedDevice: DeviceType.Current),
                new Port(60, string.Empty, false, expectedDevice: DeviceType.Voltage),
                new Port(61, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubTemperatureSensor),
                new Port(96, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubTemperatureSensor),
                new Port(97, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubAccelerometer),
                new Port(98, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubGyroSensor),
                new Port(99, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubTiltSensor),
                new Port(100, string.Empty, false, expectedDevice: DeviceType.TechnicMediumHubGestSensor),
            })
        { }

        public Port A => Port(0);
        public Port B => Port(1);
        public Port C => Port(2);
        public Port D => Port(3);

        public RgbLight RgbLight => Port(50).GetDevice<RgbLight>();
        public Current Current => Port(59).GetDevice<Current>();
        public Voltage Voltage => Port(60).GetDevice<Voltage>();
        public TechnicMediumHubTemperatureSensor Temperature1 => Port(61).GetDevice<TechnicMediumHubTemperatureSensor>();
        public TechnicMediumHubTemperatureSensor Temperature2 => Port(96).GetDevice<TechnicMediumHubTemperatureSensor>();
        public TechnicMediumHubAccelerometer Accelerometer => Port(97).GetDevice<TechnicMediumHubAccelerometer>();
        public TechnicMediumHubGyroSensor GyroSensor => Port(98).GetDevice<TechnicMediumHubGyroSensor>();
        public TechnicMediumHubTiltSensor TiltSensor => Port(99).GetDevice<TechnicMediumHubTiltSensor>();
        public TechnicMediumHubGestSensor GestureSensor => Port(100).GetDevice<TechnicMediumHubGestSensor>();
    }
}