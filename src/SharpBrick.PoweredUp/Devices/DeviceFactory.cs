using System;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory : IDeviceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DeviceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IPoweredUpDevice Create(DeviceType deviceType)
        {
            var type = GetTypeFromDeviceType(deviceType);

            return (type == null) ? null : (IPoweredUpDevice)ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }

        public IPoweredUpDevice CreateConnected(DeviceType deviceType, ILegoWirelessProtocol protocol, byte hubId, byte portId)
        {
            var type = GetTypeFromDeviceType(deviceType);

            return (type == null) ? new DynamicDevice(protocol, hubId, portId) : (IPoweredUpDevice)ActivatorUtilities.CreateInstance(_serviceProvider, type, protocol, hubId, portId);
        }

        public Type GetTypeFromDeviceType(DeviceType deviceType)
            => deviceType switch
            {
                DeviceType.Voltage => typeof(Voltage),
                DeviceType.Current => typeof(Current),
                DeviceType.RgbLight => typeof(RgbLight),
                DeviceType.SystemTrainMotor => typeof(SystemTrainMotor),
                DeviceType.TechnicLargeLinearMotor => typeof(TechnicLargeLinearMotor),
                DeviceType.TechnicXLargeLinearMotor => typeof(TechnicXLargeLinearMotor),
                DeviceType.TechnicMediumHubGestureSensor => typeof(TechnicMediumHubGestureSensor),
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
                nameof(SystemTrainMotor) => DeviceType.SystemTrainMotor,
                nameof(TechnicLargeLinearMotor) => DeviceType.TechnicLargeLinearMotor,
                nameof(TechnicXLargeLinearMotor) => DeviceType.TechnicXLargeLinearMotor,
                nameof(TechnicMediumHubGestureSensor) => DeviceType.TechnicMediumHubGestureSensor,
                nameof(TechnicMediumHubAccelerometer) => DeviceType.TechnicMediumHubAccelerometer,
                nameof(TechnicMediumHubGyroSensor) => DeviceType.TechnicMediumHubGyroSensor,
                nameof(TechnicMediumHubTiltSensor) => DeviceType.TechnicMediumHubTiltSensor,
                nameof(TechnicMediumHubTemperatureSensor) => DeviceType.TechnicMediumHubTemperatureSensor,
                _ => DeviceType.Unknown,
            };
    }
}