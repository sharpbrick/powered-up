using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

using System;

namespace SharpBrick.PoweredUp
{
    public class TwoPortHandset : Hub
    {
        public TwoPortHandset(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<TwoPortHandset> logger, IServiceProvider serviceProvider = default)
            : base(protocol, deviceFactory, logger, serviceProvider, SystemType.LegoSystem_TwoPortHandset, new Port[] {
                new Port(0, string.Empty, false, expectedDevice: DeviceType.RemoteControlButton),
                new Port(1, string.Empty, false, expectedDevice: DeviceType.RemoteControlButton),
                new Port(52, string.Empty, false, expectedDevice: DeviceType.RgbLight),
                new Port(59, string.Empty, false, expectedDevice: DeviceType.Voltage),
                new Port(60, string.Empty, false, expectedDevice: DeviceType.RemoteControlRssi),
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
                //HubProperty.SecondaryMacAddress, // unsupported on the two port handset
                HubProperty.HardwareNetworkFamily,
            })
        { }

        public RemoteControlButton A => Port(0).GetDevice<RemoteControlButton>();
        public RemoteControlButton B => Port(1).GetDevice<RemoteControlButton>();

        public RgbLight RgbLight => Port(52).GetDevice<RgbLight>();
        public Voltage Voltage => Port(59).GetDevice<Voltage>();
        public RemoteControlRssi RemoteControlRssi => Port(60).GetDevice<RemoteControlRssi>();
    }
}