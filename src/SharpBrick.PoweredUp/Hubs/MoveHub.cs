using System;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp;

public class MoveHub : Hub
{
    public MoveHub(ILegoWirelessProtocol protocol, IDeviceFactory deviceFactory, ILogger<MoveHub> logger, IServiceProvider serviceProvider = default)
        : base(protocol, deviceFactory, logger, serviceProvider, SystemType.LegoSystem_MoveHub, new Port[] {
                new Port(0, "A", false, expectedDevice: DeviceType.MoveHubInternalMotor),
                new Port(1, "B", false, expectedDevice: DeviceType.MoveHubInternalMotor),
                // Since ports C and D can be any compatible sensor or motor, we don't set an expected device type here
                new Port(2, nameof(C), true),
                new Port(3, nameof(D), true),
                new Port(16, "AB", false, expectedDevice: DeviceType.MoveHubInternalMotor, true),
                new Port(50, string.Empty, false, expectedDevice: DeviceType.RgbLight),
                new Port(58, string.Empty, false, expectedDevice: DeviceType.MoveHubTiltSensor),
                new Port(59, string.Empty, false, expectedDevice: DeviceType.Current),
                new Port(60, string.Empty, false, expectedDevice: DeviceType.Voltage),
            // Note that there is a port with id 70 but is not currently known what this does (suspected to be debug port)
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
            //HubProperty.HardwareNetworkFamily, // Does not appear to work on Move Hub
        })
    { }

    public Port C => Port(2);
    public Port D => Port(3);

    public RgbLight RgbLight => Port(50).GetDevice<RgbLight>();
    public MoveHubTiltSensor TiltSensor => Port(58).GetDevice<MoveHubTiltSensor>();
    public Current Current => Port(59).GetDevice<Current>();
    public Voltage Voltage => Port(60).GetDevice<Voltage>();

    /// <summary>
    /// This is the virtual port of the motors built into the MoveHub.  This controls both left and right motors
    /// </summary>
    public MoveHubInternalMotor MotorAtAB => Port(16).GetDevice<MoveHubInternalMotor>();

    /// <summary>
    /// This is the motor built into the MoveHub controlling the left motor (B)
    /// </summary>
    public MoveHubInternalMotor LeftMotorAtB => Port(1).GetDevice<MoveHubInternalMotor>();

    /// <summary>
    /// This is the motor built into the MoveHub controlling the right motor (A)
    /// </summary>
    public MoveHubInternalMotor RightMotorAtA => Port(0).GetDevice<MoveHubInternalMotor>();
}
