using System;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPowerdUpDevice Create(HubAttachedIOType type)
            => (IPowerdUpDevice)Activator.CreateInstance(GetTypeOfDevice(type));

        public static IPowerdUpDevice CreateConnected(HubAttachedIOType type, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => (IPowerdUpDevice)Activator.CreateInstance(GetTypeOfDevice(type), protocol, hubId, portId);

        private static Type GetTypeOfDevice(HubAttachedIOType type)
            => type switch
            {
                HubAttachedIOType.Voltage => typeof(Voltage),
                HubAttachedIOType.Current => typeof(Current),
                HubAttachedIOType.RgbLight => typeof(RgbLight),
                HubAttachedIOType.TechnicLargeLinearMotor => typeof(TechnicLargeLinearMotor),
                HubAttachedIOType.TechnicXLargeLinearMotor => typeof(TechnicXLargeLinearMotor),
                HubAttachedIOType.TechnicMediumHubGestSensor => typeof(TechnicMediumHubGestSensor),
                HubAttachedIOType.TechnicMediumHubAccelerometer => typeof(TechnicMediumHubAccelerometer),
                HubAttachedIOType.TechnicMediumHubGyroSensor => typeof(TechnicMediumHubGyroSensor),
                HubAttachedIOType.TechnicMediumHubTiltSensor => typeof(TechnicMediumHubTiltSensor),
                HubAttachedIOType.TechnicMediumHubTemperatureSensor => typeof(TechnicMediumHubTemperatureSensor),
                _ => null,
            };
    }
}