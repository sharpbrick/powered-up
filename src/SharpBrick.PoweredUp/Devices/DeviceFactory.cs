using System;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPoweredUpDevice Create(DeviceType type)
            => (IPoweredUpDevice)Activator.CreateInstance(GetTypeOfDevice(type));

        public static IPoweredUpDevice CreateConnected(DeviceType type, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => (IPoweredUpDevice)Activator.CreateInstance(GetTypeOfDevice(type), protocol, hubId, portId);

        private static Type GetTypeOfDevice(DeviceType type)
            => type switch
            {
                DeviceType.Voltage => typeof(Voltage),
                DeviceType.Current => typeof(Current),
                DeviceType.RgbLight => typeof(RgbLight),
                DeviceType.TechnicLargeLinearMotor => typeof(TechnicLargeLinearMotor),
                DeviceType.TechnicXLargeLinearMotor => typeof(TechnicXLargeLinearMotor),
                DeviceType.TechnicMediumHubGestSensor => typeof(TechnicMediumHubGestSensor),
                DeviceType.TechnicMediumHubAccelerometer => typeof(TechnicMediumHubAccelerometer),
                DeviceType.TechnicMediumHubGyroSensor => typeof(TechnicMediumHubGyroSensor),
                DeviceType.TechnicMediumHubTiltSensor => typeof(TechnicMediumHubTiltSensor),
                DeviceType.TechnicMediumHubTemperatureSensor => typeof(TechnicMediumHubTemperatureSensor),
                _ => null,
            };
    }
}