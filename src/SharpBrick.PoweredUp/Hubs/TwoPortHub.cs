using System;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp;

public class TwoPortHub : Hub
{
    public TwoPortHub(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<TwoPortHub> logger, IServiceProvider serviceProvider = default)
        : base(protocol, deviceFactory, logger, serviceProvider, SystemType.LegoSystem_TwoPortHub, new Port[] {
                new Port(0, nameof(A), true),
                new Port(1, nameof(B), true),
                new Port(50, string.Empty, false, expectedDevice: DeviceType.RgbLight),
                new Port(59, string.Empty, false, expectedDevice: DeviceType.Current),
                new Port(60, string.Empty, false, expectedDevice: DeviceType.Voltage),
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
                HubProperty.SecondaryMacAddress,
            //HubProperty.HardwareNetworkFamily, // support status unknown for TwoPortHub
        })
    { }

    public Port A => Port(0);
    public Port B => Port(1);

    public RgbLight RgbLight => Port(50).GetDevice<RgbLight>();
    public Current Current => Port(59).GetDevice<Current>();
    public Voltage Voltage => Port(60).GetDevice<Voltage>();
}
