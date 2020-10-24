using System;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp
{
    public class MarioHub : Hub
    {
        public MarioHub(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<MarioHub> logger, IServiceProvider serviceProvider = default)
            : base(protocol, deviceFactory, logger, serviceProvider, SystemType.LegoSystem_Mario, new Port[] {
                new Port(0, string.Empty, false, expectedDevice: DeviceType.MarioHubAccelerometer),
                new Port(1, string.Empty, false, expectedDevice: DeviceType.MarioHubTagSensor),
                new Port(2, string.Empty, false, expectedDevice: DeviceType.MarioHubPants),
                new Port(3, string.Empty, false, expectedDevice: DeviceType.MarioHubDebug),
                new Port(6, string.Empty, false, expectedDevice: DeviceType.Voltage),
            },
            knownProperties: new HubProperty[] {
                HubProperty.AdvertisingName,
                HubProperty.Button,
                HubProperty.FwVersion,
                HubProperty.HwVersion,
                HubProperty.Rssi,
                HubProperty.BatteryVoltage,
                HubProperty.BatteryType,
                HubProperty.ManufacturerName,
                HubProperty.RadioFirmwareVersion,
                HubProperty.LegoWirelessProtocolVersion,
                HubProperty.SystemTypeId,
                // HubProperty.HardwareNetworkId, // Throws command not recognized error for MarioHub
                HubProperty.PrimaryMacAddress,
                HubProperty.SecondaryMacAddress,
                //HubProperty.HardwareNetworkFamily, // may throw command not recognized error for MarioHub
            })
        { }

        public MarioHubAccelerometer Accelerometer => Port(0).GetDevice<MarioHubAccelerometer>();
        public MarioHubTagSensor TagSensor => Port(1).GetDevice<MarioHubTagSensor>();
        public MarioHubPants Pants => Port(2).GetDevice<MarioHubPants>();
        public MarioHubDebug Debug => Port(3).GetDevice<MarioHubDebug>();
        public Voltage Voltage => Port(6).GetDevice<Voltage>();
    }
}