using System;
using SharpBrick.PoweredUp.Protocol.Messages;

namespace SharpBrick.PoweredUp.Devices
{
    public class DeviceFactory
    {
        public static IPowerdUpDevice Create(HubAttachedIOType type)
            => type switch
            {
                HubAttachedIOType.TechnicMediumHubTiltSensor => new TechnicMediumHubTiltSensor(),
                HubAttachedIOType.TechnicLargeLinearMotor => new TechnicLargeLinearMotor(),
                HubAttachedIOType.TechnicXLargeLinearMotor => new TechnicXLargeLinearMotor(),
                _ => null,
            };
    }
}