# SharpBrick.PoweredUp

SharpBrick.PoweredUp is a .NET implementation of the Bluetooth Low Energy Protocol for Lego Powered Up products.

[![Nuget](https://img.shields.io/nuget/v/SharpBrick.PoweredUp?style=flat-square)](https://www.nuget.org/packages/SharpBrick.PoweredUp/)
![license:MIT](https://img.shields.io/github/license/sharpbrick/powered-up?style=flat-square)
[![GitHub issues by-label](https://img.shields.io/github/issues/sharpbrick/powered-up/bug?color=red&style=flat-square)](https://github.com/sharpbrick/powered-up/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![GitHub issues by-label](https://img.shields.io/github/issues/sharpbrick/powered-up/enhancement?color=green&style=flat-square)](https://github.com/sharpbrick/powered-up/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)


![Build-CI](https://github.com/sharpbrick/powered-up/workflows/Build-CI/badge.svg)
![Build-Release](https://github.com/sharpbrick/powered-up/workflows/Build-Release/badge.svg)

# Examples

***Note**: While already usable, this library is work in progress.*

## Discovering Hubs

````csharp
var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

var host = new PoweredUpHost(poweredUpBluetoothAdapter);

var cts = new CancellationTokenSource();
host.Discover(async hub =>
{
    await hub.ConnectAsync();

    Console.WriteLine(hub.AdvertisingName);
    Console.WriteLine(hub.SystemType.ToString());

    cts.Cancel();
    Console.WriteLine("Press RETURN to continue");
}, cts.Token);

Console.WriteLine("Press RETURN to cancel Scanning");
Console.ReadLine();

cts.Cancel();
````

## Sending Commands to Ports and Devices of a Hub

See source code in `examples/SharpBrick.PoweredUp.Examples` for more examples.

````csharp
var host = new PoweredUpHost();

// do discovery before

using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
{
    await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

    var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

    await motor.GotoAbsolutePositionAsync(45, 10, 100, PortOutputCommandSpecialSpeed.Brake, PortOutputCommandSpeedProfile.None);
    await Task.Delay(2000);
    await motor.GotoAbsolutePositionAsync(-45, 10, 100, PortOutputCommandSpecialSpeed.Brake, PortOutputCommandSpeedProfile.None);

    await technicMediumHub.SwitchOffAsync();
}
````

## Receiving values from Ports and Devices of a Hub (single value setup)

***Note**: All API calls are currently not synchronized (e.g. the result property is not filled just because the request was sent)*

````csharp
var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true);

// observe using System.Reactive
var disposable = motor.AbsolutePositionObservable.Subscribe(x => Console.WriteLine(x.SI));
// ... once finished observing (do not call immediately afterwards ;))
disposable.Dispose();

// OR manually observe it
Console.WriteLine(motor.AbsolutePosition);
````

## Discover Hubs (using raw bluetooth kernel)

````csharp
var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

// Finding Service
ulong bluetoothAddress = 0;

var cts = new CancellationTokenSource();

poweredUpBluetoothAdapter.Discover(info =>
{
    Console.WriteLine($"Found {info.BluetoothAddress} is a {(PoweredUpManufacturerDataConstants)info.ManufacturerData[1]}");

    bluetoothAddress = info.BluetoothAddress;
}, cts.Token);

Console.WriteLine("Press any key to cancel Scanning");
Console.ReadLine();

cts.Cancel(); // cancels the scanning process
````

## Connect to Hub and Send a Message and retrieving answers (using raw bluetooth kernel and messages)

````csharp
using (var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()))
{    
    var protocol = new PoweredUpProtocol(kernel);

    await kernel.ConnectAsync();
    await protocol.ConnectAsync();
    
    using disposable = protocol.UpstreamMessages.Subscribe(message =>
    {
        if (message is HubPropertyMessage<string> msg)
        {
            Console.WriteLine($"Hub Property - {msg.Property}: {msg.Payload}");
        }
    });

    await protocol.SendMessageAsync(new HubPropertyMessage() { 
        Property = HubProperty.AdvertisingName, 
        Operation = HubPropertyOperation.RequestUpdate
    });

    Console.Readline(); // allow the messages to be processed and displayed.

    // fun with light on hub 0 and built-in LED on port 50
    var rgbLight = new RgbLight(protocol, 0, 50);
    await rgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

    // fun with motor on hub 0 and port 0
    var motor = new TechnicXLargeLinearMotor(protocol, 0, 0);
    await motor.GotoAbsolutePositionAsync(45, 10, 100, PortOutputCommandSpecialSpeed.Brake, PortOutputCommandSpeedProfile.None);
    await Task.Delay(2000);
    await motor.GotoAbsolutePositionAsync(-45, 10, 100, PortOutputCommandSpecialSpeed.Brake, PortOutputCommandSpeedProfile.None);
}
````

## Implementation Status

- Bluetooth Adapter
  - [X] .NET Core 3.1 (on Windows 10 using WinRT)
    - Library uses `Span<T>` therefore not supported in .NET Framework 1.0-4.8 and UWP Apps until arrival of .NET 5 (WinForms and WPF work in .NET Core 3.1)
    - Library uses WinRT for communication therefore only Windows 10
  - [ ] Xamarin (on iOS / Android using Xamarin.Essentials)
  - [ ] Blazor (on Browser using WebBluetooth)
- Hub Model
  - Hubs
    - [X] Ports
    - [X] Properties
    - [ ] Alerts
    - [ ] Actions
    - [X] Create Virtual Ports
    - [X] Technic Medium Hub
    - .. other hubs depend on availability of hardware / contributions
  - Devices
    - [X] Technic Medium Hub - Rgb Light
    - [X] Technic Medium Hub - Current
    - [X] Technic Medium Hub - Voltage
    - [X] Technic Medium Hub - Temperature Sensor 1 + 2
    - [X] Technic Medium Hub - Accelerometer
    - [X] Technic Medium Hub - Gyro Sensor
    - [ ] Technic Medium Hub - Tilt Sensor
    - [ ] Technic Medium Hub - Gesture Sensor
    - [X] Technic XLarge Motor
    - [X] Technic Large Motor
- Message Encoder
  - [X] [3.1 Common Message Header](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#common-message-header)
  - [X] [3.2. Message Length Encoding](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#message-length-encoding)
  - [X] [3.3. Message Types (Enum)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#message-types)
  - [ ] [3.4 Transmission Flow Control (Preliminary PROPOSAL!)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#transmission-flow-control-preliminary-proposal)
  - [X] [3.5. Hub Properties](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-properties)
  - [X] [3.6. Hub Actions](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-actions)
  - [X] [3.7. Hub Alerts](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-alerts)
  - [X] [3.8. Hub Attached I/O](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#hub-attached-i-o)
  - [X] [3.9. Generic Error Messages](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#generic-error-messages)
  - [ ] ~~[3.10. H/W NetWork Commands](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#h-w-network-commands)~~
  - [ ] ~~[3.11. F/W Update - Go Into Boot Mode](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-go-into-boot-mode)~~
  - [ ] ~~[3.12. F/W Update Lock memory](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-lock-memory)~~
  - [ ] ~~[3.13. F/W Update Lock Status Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-update-lock-status-request)~~
  - [ ] ~~[3.14. F/W Lock Status](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#f-w-lock-status)~~
  - [X] [3.15. Port Information Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-information-request)
  - [X] [3.16. Port Mode Information Request](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-mode-information-request)
  - [X] [3.17. Port Input Format Setup (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-setup-single)
  - [X] [3.18. Port Input Format Setup (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-setup-combinedmode)
  - [X] [3.19. Port Information](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-information)
  - [X] [3.20. Port Mode Information](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-mode-information)
  - [X] [3.21. Port Value (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-value-single)
  - [X] [3.22. Port Value (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-value-combinedmode)
  - [X] [3.23. Port Input Format (Single)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-single)
  - [X] [3.24. Port Input Format (CombinedMode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-input-format-combinedmode)
  - [X] [3.25. Virtual Port Setup](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#virtual-port-setup)
  - [X] [3.26. Port Output Command](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-output-command)
  - [3.27. Output Command 0x81 - Motor Sub Commands [0x01-0x3F]](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-command-0x81-motor-sub-commands-0x01-0x3f)
    - [X] [3.27.1 - StartPower(Power)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startpower-power)
    - [X] [3.27.2 - StartPower(Power1, Power2)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startpower-power1-power2-0x02)
    - [X] [3.27.3 - SetAccTime (Time, ProfileNo)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-setacctime-time-profileno-0x05)
    - [X] [3.27.4 - SetDecTime (Time, ProfileNo)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-setdectime-time-profileno-0x06)
    - [X] [3.27.5 - StartSpeed (Speed, MaxPower, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeed-speed-maxpower-useprofile-0x07)
    - [X] [3.27.6 - StartSpeed (Speed1, Speed2, MaxPower, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeed-speed1-speed2-maxpower-useprofile-0x08)
    - [X] [3.27.7 - StartSpeedForTime (Time, Speed, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeedfortime-time-speed-maxpower-endstate-useprofile-0x09)
    - [X] [3.27.8 - StartSpeedForTime (Time, SpeedL, SpeedR, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeedfortime-time-speed-maxpower-endstate-useprofile-0x0a)
    - [X] [3.27.9 - StartSpeedForDegrees(Degrees, Speed, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeedfordegrees-degrees-speed-maxpower-endstate-useprofile-0x0b)
    - [X] [3.27.10 - StartSpeedForDegrees(Degrees, SpeedL, SpeedR, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-startspeedfordegrees-degrees-speedl-speedr-maxpower-endstate-useprofile-0x0c)
    - [X] [3.27.11 - GotoAbsolutePosition(AbsPos, Speed, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-gotoabsoluteposition-abspos-speed-maxpower-endstate-useprofile-0x0d)...
    - [X] [3.27.12 - GotoAbsolutePosition(AbsPos1, AbsPos2, Speed, MaxPower, EndState, UseProfile)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-gotoabsoluteposition-abspos1-abspos2-speed-maxpower-endstate-useprofile-0x0e)
    - [ ] [3.27.13 - PresetEncoder(Position)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-presetencoder-position-n-a)
    - [ ] [3.27.14 - PresetEncoder(LeftPosition, RightPosition)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-presetencoder-leftposition-rightposition-0x14)
    - [ ] [3.27.15 - TiltImpactPreset(PresetValue)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-tiltimpactpreset-presetvalue-n-a)
    - [ ] [3.27.16 - TiltConfigOrientation(Orientation)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-tiltconfigorientation-orientation-n-a)
    - [ ] [3.27.17 - TiltConfigImpact(ImpactThreshold, BumpHoldoff)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-tiltconfigimpact-impactthreshold-bumpholdoff-n-a)
    - [ ] [3.27.18 - TiltFactoryCalibration(Orientation, PassCode)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-tiltfactorycalibration-orientation-passcode-n-a)
    - [ ] [3.27.19 - GenericZeroSetHardware()](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-hardware-reset-genericzerosethardware-n-a)
    - [X] [3.27.20 - SetRgbColorNo(ColorNo)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-setrgbcolorno-colorno-n-a)
    - [X] [3.27.21 - SetRgbColorNo(RedColor, GreenColor, BlueColor)](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#output-sub-command-setrgbcolorno-redcolor-greencolor-bluecolor-n-a)
  - [ ] [3.28. WriteDirect](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#writedirect)
  - [X] [3.29. WriteDirectModeData](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#writedirectmodedata)
  - [ ] [3.30. Checksum Calculation](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#checksum-calculation)
  - [ ] [3.31. Tacho Math](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#tacho-math)
  - [X] [3.32. Port Output Command Feedback](https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#port-output-command-feedback)

## SDKs in other programming languages

- JavaScript (Node + Browser): 
  - [nathankellenicki/node-poweredup](https://github.com/nathankellenicki/node-poweredup)
- .NET / C#
  - **sharpbrick/powered-up** (this here)
  - [Vouzamo/Lego](https://github.com/Vouzamo/Lego) (and [blog](https://vouzamo.wordpress.com/2020/04/21/lego-c-sdk-enhancements-challenges/))
- C++
  -  [corneliusmunz/legoino](https://github.com/corneliusmunz/legoino) (Arduino)
- Python
  - [alexsdutton/python-lego-wireless-protocol](https://github.com/alexsdutton/python-lego-wireless-protocol)
  - [undera/pylgbst](https://github.com/undera/pylgbst)
  - [virantha/bricknil](https://github.com/virantha/bricknil/)

## Resources

- [Lego Wireless Protocol Specification](https://lego.github.io/lego-ble-wireless-protocol-docs) ([GitHub](https://github.com/lego/lego-ble-wireless-protocol-docs))

## Contribution

SharpBrick is an organization intended to host volunteers willing to contribute to the SharpBrick.PoweredUp and related projects. Everyone is welcome (private and commercial entities). Please read our **[Code of Conduct](CODE_OF_CONDUCT.md)** before participating in our project.

The product is licensed under **MIT License** to allow a easy and wide adoption into prviate and commercial products.
