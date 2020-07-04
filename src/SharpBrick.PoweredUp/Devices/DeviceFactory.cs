using System;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPoweredUpDevice Create(DeviceType type)
            => (IPoweredUpDevice)Activator.CreateInstance(GetTypeFromDeviceType(type));

        public static IPoweredUpDevice CreateConnected(DeviceType type, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => (IPoweredUpDevice)Activator.CreateInstance(GetTypeFromDeviceType(type), protocol, hubId, portId);

        public static Type GetTypeFromDeviceType(DeviceType deviceType)
            => deviceType switch
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

        public static DeviceType GetDeviceTypeFromType(Type type)
            => type.Name switch // fuzzy but will work
            {
                nameof(Voltage) => DeviceType.Voltage,
                nameof(Current) => DeviceType.Current,
                nameof(RgbLight) => DeviceType.RgbLight,
                nameof(TechnicLargeLinearMotor) => DeviceType.TechnicLargeLinearMotor,
                nameof(TechnicXLargeLinearMotor) => DeviceType.TechnicXLargeLinearMotor,
                nameof(TechnicMediumHubGestSensor) => DeviceType.TechnicMediumHubGestSensor,
                nameof(TechnicMediumHubAccelerometer) => DeviceType.TechnicMediumHubAccelerometer,
                nameof(TechnicMediumHubGyroSensor) => DeviceType.TechnicMediumHubGyroSensor,
                nameof(TechnicMediumHubTiltSensor) => DeviceType.TechnicMediumHubTiltSensor,
                nameof(TechnicMediumHubTemperatureSensor) => DeviceType.TechnicMediumHubTemperatureSensor,
                _ => DeviceType.Unknown,
            };
    }
}