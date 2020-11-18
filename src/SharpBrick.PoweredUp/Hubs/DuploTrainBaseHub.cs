using System;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp
{
    public class DuploTrainBaseHub : Hub
    {
        public DuploTrainBaseHub(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<DuploTrainBaseHub> logger, IServiceProvider serviceProvider = default)
            : base(protocol, deviceFactory, logger, serviceProvider, SystemType.LegoDuplo_DuploTrain, new Port[] {
                new Port(0, string.Empty, false, expectedDevice: DeviceType.DuploTrainBaseMotor),
                new Port(1, string.Empty, false, expectedDevice: DeviceType.DuploTrainBaseSpeaker),
                new Port(17, string.Empty, false, expectedDevice: DeviceType.RgbLight),
                new Port(18, string.Empty, false, expectedDevice: DeviceType.DuploTrainBaseColorSensor),
                new Port(19, string.Empty, false, expectedDevice: DeviceType.DuploTrainBaseSpeedometer ),
                new Port(20, string.Empty, false, expectedDevice: DeviceType.Voltage),
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
                HubProperty.HardwareNetworkId,
                HubProperty.PrimaryMacAddress,
                // HubProperty.SecondaryMacAddress, // throws
                // HubProperty.HardwareNetworkFamily, // throws
            })
        { }

        public DuploTrainBaseMotor Motor => Port(0).GetDevice<DuploTrainBaseMotor>();
        public DuploTrainBaseSpeaker Speaker => Port(1).GetDevice<DuploTrainBaseSpeaker>();
        public RgbLight RgbLight => Port(17).GetDevice<RgbLight>();
        public DuploTrainBaseSpeedometer Speedometer => Port(19).GetDevice<DuploTrainBaseSpeedometer>();
        public Voltage Voltage => Port(20).GetDevice<Voltage>();
    }
}