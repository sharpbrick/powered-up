using System;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPowerdUpDevice Create(DeviceType type)
            => (IPowerdUpDevice)Activator.CreateInstance(GetTypeOfDevice(type));

        public static IPowerdUpDevice CreateConnected(DeviceType type, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => (IPowerdUpDevice)Activator.CreateInstance(GetTypeOfDevice(type), protocol, hubId, portId);

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