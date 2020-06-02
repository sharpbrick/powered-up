using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPowerdUpDevice Create(HubAttachedIOType type)
            => type switch
            {
                HubAttachedIOType.Voltage => new Voltage(),
                HubAttachedIOType.Current => new Current(),
                HubAttachedIOType.RgbLight => new RgbLight(),
                HubAttachedIOType.TechnicLargeLinearMotor => new TechnicLargeLinearMotor(),
                HubAttachedIOType.TechnicXLargeLinearMotor => new TechnicXLargeLinearMotor(),
                HubAttachedIOType.TechnicMediumHubGestSensor => new TechnicMediumHubGestSensor(),
                HubAttachedIOType.TechnicMediumHubAccelerometer => new TechnicMediuimHubAccelerometer(),
                HubAttachedIOType.TechnicMediumHubGyroSensor => new TechnicMediumHubGyroSensor(),
                HubAttachedIOType.TechnicMediumHubTiltSensor => new TechnicMediumHubTiltSensor(),
                HubAttachedIOType.TechnicMediumHubTemperatureSensor => new TechnicMediumHubTemperatureSensor(),
                _ => null,
            };
    }
}