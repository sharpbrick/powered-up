# SharpBrick.PoweredUp

SharpBrick.PoweredUp is a .NET implementation of the Bluetooth Low Energy Protocol for Lego Powered UP products.

[![Nuget](https://img.shields.io/nuget/v/SharpBrick.PoweredUp?style=flat-square)](https://www.nuget.org/packages/SharpBrick.PoweredUp/)
![license:MIT](https://img.shields.io/github/license/sharpbrick/powered-up?style=flat-square)
[![GitHub issues by-label](https://img.shields.io/github/issues/sharpbrick/powered-up/bug?color=red&style=flat-square)](https://github.com/sharpbrick/powered-up/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![GitHub issues by-label](https://img.shields.io/github/issues/sharpbrick/powered-up/enhancement?color=green&style=flat-square)](https://github.com/sharpbrick/powered-up/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)


![Build-CI](https://github.com/sharpbrick/powered-up/workflows/Build-CI/badge.svg)
![Build-Release](https://github.com/sharpbrick/powered-up/workflows/Build-Release/badge.svg)

# Features

- **Multiple Programming Models**: SharpBrick.PoweredUp supports usage in a *device model* (hubs and devices as classes/properties; see examples below) or a *protocol level* (messages send up and down the Bluetooth Low Energy Protocol).
- **Typed Devices with explicit Functions**: The SDK supports most commands described in the Lego Wireless Protocol in its typed devices (Motors, Lights, ..). They are self-describing to support a quick bootup of the SDK.
- **Dynamic Devices**: The SDK can auto-discover new devices which are not yet known by the SDK. The device can be directly accessed either by writing data directly to a mode or receiving notification about value changes.
- **Awaitable Commands**: Instead of waiting a defined amount of time for the devices to react, directly listens to the feedback messages the LEGO Wireless Protocol provides. No unecessary delays and race conditions.
- **Port Value Combined Mode**: If supported by the device, the SDK allows you to configure the devices to combine multiple feedbacks of the same device within the same message (e.g. speed and absolute position of a motor).
- **Virtual Port Creation**: Combine multiple devices of the same type into a virtual combined port. This allows synchronous access to multiple devices using the same message (e.g. using two motors for driving).
- **Deployment Model Verification**: The SDK includes a model builder and a verification method to ensure that the wired devies are correctly reflecting the expectations in the program.
- **Tools**: The `poweredup` CLI includes a device list feature, enumerating the metadata properties of the LEGO Wireless Protocol.
- **BlueGiga Bluetooth-Adapter Support**: With the SharpBrick.PoweredUp.BlueGigaBLE-package you can use a Silicon's Lab BlueGiga-adapter (for example BLED112) to talk to your Lego-Hubs. By default the WinRT-implementation for build-in bluetooth-adapters is used.

# Examples

Additional to code fragments below, look into the `examples/SharpBrick.PoweredUp.Examples` project (15+ examples).

````csharp
using SharpBrick.PoweredUp;
````

## Discovering Hubs

````csharp
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT)
    .BuildServiceProvider();
    
var host = serviceProvider.GetService<PoweredUpHost>();

var hub = await host.DiscoverAsync<TechnicMediumHub>();
await hub.ConnectAsync();
````

## Discovering Hubs for UI
````csharp
var host = serviceProvider.GetService<PoweredUpHost>();

var cts = new CancellationTokenSource();
host.Discover(async hub =>
{
    await hub.ConnectAsync(); // to get some more properties from it

    // show in UI
}, cts.Token);

// Cancel Button => cts.Cancel();
````

## Sending Commands to Ports and Devices of a Hub

See source code in `examples/SharpBrick.PoweredUp.Examples` for more examples.

````csharp
// do hub discovery before

using (var technicMediumHub = hub as TechnicMediumHub)
{
    // optionally verify if everything is wired up correctly
    await technicMediumHub.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
        .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
            .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub.A)
        )
    );

    await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

    var motor = technicMediumHub.A.GetDevice<TechnicXLargeLinearMotor>();

    await motor.GotoPositionAsync(45, 10, 100, PortOutputCommandSpecialSpeed.Brake);
    await Task.Delay(2000);
    await motor.GotoPositionAsync(-45, 10, 100, PortOutputCommandSpecialSpeed.Brake);

    await technicMediumHub.SwitchOffAsync();
}
````

## Receiving values from Ports and Devices of a Hub (single value setup)

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

## Connecting to an unknown device

````csharp
// deployment model verification with unknown devices
await technicMediumHub.VerifyDeploymentModelAsync(mb => mb
    .AddAnyHub(hubBuilder => hubBuilder
        .AddAnyDevice(0))
    );

var dynamicDeviceWhichIsAMotor = technicMediumHub.Port(0).GetDevice<DynamicDevice>();

// or also direct from a protocol
//var dynamicDeviceWhichIsAMotor = new DynamicDevice(technicMediumHub.Protocol, technicMediumHub.HubId, 0);

// discover the unknown device using the LWP (since no cached metadata available)
await dynamicDeviceWhichIsAMotor.DiscoverAsync();

// use combined mode values from the device
await dynamicDeviceWhichIsAMotor.TryLockDeviceForCombinedModeNotificationSetupAsync(2, 3);
await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(2, true);
await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(3, true);
await dynamicDeviceWhichIsAMotor.UnlockFromCombinedModeNotificationSetupAsync(true);

// get the individual modes for input and output
var powerMode = dynamicDeviceWhichIsAMotor.SingleValueMode<sbyte>(0);
var posMode = dynamicDeviceWhichIsAMotor.SingleValueMode<int>(2);
var aposMode = dynamicDeviceWhichIsAMotor.SingleValueMode<short>(3);

// use their observables to report values
using var disposable = posMode.Observable.Subscribe(x => Console.WriteLine($"Position: {x.SI} / {x.Pct}"));
using var disposable2 = aposMode.Observable.Subscribe(x => Console.WriteLine($"Absolute Position: {x.SI} / {x.Pct}"));

// or even write to them
await powerMode.WriteDirectModeDataAsync(0x64); // That is StartPower(100%) on a motor
await Task.Delay(2_000);
await powerMode.WriteDirectModeDataAsync(0x00); // That is Stop on a motor

Console.WriteLine($"Or directly read the latest value: {aposMode.SI} / {aposMode.Pct}%");
````

## Connect to Hub and Send a Message and retrieving answers (directly on protocol layer)

***Note**: The `ILegoWirelessProtocol` class was renamed in 3.0. Previously it is known as `IPoweredUpProtocol`.*

````csharp

var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT)
    .BuildServiceProvider();

using (var scope = serviceProvider.CreateScope()) // create a scoped DI container per intented active connection/protocol. If disposed, disposes all disposable artifacts.
{
    // init BT layer with right bluetooth address
    scope.ServiceProvider.GetService<BluetoothKernel>().BluetoothAddress = bluetoothAddress;

    var protocol = scope.GetService<ILegoWirelessProtocol>();

    await protocol.ConnectAsync(); // also connects underlying BT connection
    
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

    Console.Readline(); // allow the messages to be processed and displayed. (alternative: SendMessageReceiveResultAsync, SendPortOutputCommandAsync, ..)

    // fun with light on hub 0 and built-in LED on port 50
    var rgbLight = new RgbLight(protocol, 0, 50);
    await rgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

    // fun with motor on hub 0 and port 0
    var motor = new TechnicXLargeLinearMotor(protocol, 0, 0);
    await motor.GotoPositionAsync(45, 10, 100, PortOutputCommandSpecialSpeed.Brake);
    await Task.Delay(2000);
    await motor.GotoPositionAsync(-45, 10, 100, PortOutputCommandSpecialSpeed.Brake);
}
````
## Connecting with a BlueGiga-Bluetooth-adapter
If not yet installed, install the nuget-package `SharpBrick.PoweredUp.BlueGigaBLE` .

In the above examples replace
````csharp
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT)
    .BuildServiceProvider();
````
with
````csharp
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddBlueGigaBLEBluetooth(options =>
    {
      // enter the COMPort-Name here
      // on Windows-PCs you can find it under Device Manager --> Ports (COM & LPT) 
      // --> Bluegiga Bluetooth Low Energy (COM#) (where # is a number)
      options.COMPortName = "COM4";
      // setting this option to false supresses the complete LogDebug()-commands
      // of the Bluegiga-stack; so they will not generated at all
      // this produces A LOT trace-messages :-)
      options.TraceDebug = true;
    })
    .BuildServiceProvider();
````
***Note**: see also in `examples/SharpBrick.PoweredUp.Examples/BaseExample.cs` and `examples/SharpBrick.PoweredUp.Examples/Program.cs` for getting an idea of how to switch to BlueGiga-adapter by using program-arguments.*

# Command Line Experience

The `poweredup` command line utility intends to allow the inspection of LEGO Wireless Protocol / Powered UP hubs and devices for their properties. It has utilities for ...

- **Enumerating all connected Devices** including hub internal devices and emit their static self-description as they expose using the LEGO Wireless Protocol.
   ````
   poweredup device list
   ````
- **Binary dumping the self-description** helps protocol implementors with a lack of devices to understand and try to implement the devices without having the physical device. Also the output is needed when programming the library to enable a fast bootup of the SDK.
  ````
  poweredup device dump-static-port -p 0
  ````
- **Pretty Print Binary Dumps**: Help to convert a binary dump in a nice representation.

***Note**: Currently per default on Windows the WinRT Bluetooth drivers are used. Work is on the way to support bluez to run the utility also on Linux. If you've got a BlueGiga-Bluetooth-adapter, you can use the following additional options (not yet tested under Linux, but Windows).*
````
   poweredup device list --usebluegiga COM4 --tracebluegiga
   ````
Use the COM-port of your Windows-instance where the BlueGiga-adapter is attached to; so replace ````COM4```` with COMx whatever x is used on your system.
````--tracebluegiga````  emmits a lot of additional trace-information of the BleuGiga-implementation. 
## Installation Instruction

1. Install the [latest .NET](https://dotnet.microsoft.com/download) on your machine (e.g. .NET 5).
2. Install the `poweredup` dotnet utility using the following instruction
   ````
   dotnet tool install -g SharpBrick.PoweredUp.Cli
   ````
3. Start using the tool
   ````
   poweredup device list
   ````


# SDK Status, Hardware Support, Contributions, ..

Basic Architecture within the SDK
````
+---------+
|         |
| Devices | <-+
|         |   |   +-----------------------+     +-------------+     +-----+
+---------+   +-> |                       |     |             |     |     |
                  | ILegoWirelessProtocol | <-> | BLE Adapter | <-> | BLE |
+---------+   +-> |    (w/ Knowlege)      |     |             |     |     |
|         |   |   +-----------------------+     +-------------+     +-----+
|   Hub   | <-+
|         |
+---------+
````


DI Container Elements

````
                                              PoweredUpHost +-------+
                                                   +                |
                                                   |                |
+-------------------- Scoped Service Provider ------------------------+
|                                                  |                | |
|                                                  v                +--->IPoweredUp
| LinearMidCalibration +                         HubFactory         | |  BluetoothAdapter
|                      |                                            | |
| TechnicMediumHub +---+-> LegoWirelessProtocol +-> BluetoothKernel + |
|             +                       +                               |
|             |                       |                               |
|             +-----------------------+--------> DeviceFactory        |
|                                                                     |
+---------------------------------------------------------------------+
````

## Implementation Status

- Bluetooth Adapter
  - [X] .NET Core 3.1 (on Windows 10 using WinRT Bluetooth). Please use version v3.4.0 and consider upgrading to .NET 5
  - [X] .NET 5 (on Windows 10 using WinRT Bluetooth)(⚠ v4.0 or later)
  - [X] .NET 5 (on Windows 10 using BlueGiga-adapter)(⚠ v4.0 or later)
  - [ ] UWP (most likely December 2021; UWP currently does not support .NET Standard 2.1 and C# 8.0+)
  - [ ] .NET Framework 4.8 (will never be supported; .NET Framework does not and will never support .NET Standard 2.1 and C# 8.0+)
  - [X] Xamarin 5 (on Android using BLE.Plugin) (⚠ v4.0 or later)
  - [ ] Blazor/WebAssembly (on Browser using WebBluetooth; currently blocked by less than ideal GATT support on browsers, see [1](https://stackoverflow.com/questions/63757642/webbluetooth-gatt-characteric-notification-setup-too-slow-how-to-improve-setup), [2](https://github.com/WebBluetoothCG/web-bluetooth/issues/514) and [3](https://github.com/LEGO/lego-ble-wireless-protocol-docs/issues/29) )
- Hub Model
  - Hubs
    - [X] Ports
    - [X] Properties
    - [X] Alerts
    - [X] Actions
    - [X] Create Virtual Ports
    - [X] Move Hub (88006)
    - [X] Two Port Hub (88009)
    - [X] Two Port Handset (88010)
    - [X] Technic Medium Hub (88012)
    - [X] MarioHub (set 71360)
    - [X] Duplo Train Base (set 10874)
    - .. other hubs depend on availability of hardware / contributions
  - Devices
    - [X] Technic Medium Hub (88012) - Rgb Light
    - [X] Technic Medium Hub (88012) - Current
    - [X] Technic Medium Hub (88012) - Voltage
    - [X] Technic Medium Hub (88012) - Temperature Sensor 1 + 2
    - [X] Technic Medium Hub (88012) - Accelerometer
    - [X] Technic Medium Hub (88012) - Gyro Sensor
    - [X] Technic Medium Hub (88012) - Tilt Sensor
    - [X] Technic Medium Hub (88012) - Gesture Sensor (⚠ Usable but Gesture mapping is pending)
    - [X] Move Hub (88006) - Rgb Light
    - [X] Move Hub (88006) - Current
    - [X] Move Hub (88006) - Voltage
    - [X] Move Hub (88006) - Tilt
    - [X] Move Hub (88006) - Internal Motors (Single and Virtual Port)
    - [X] Two Port Hub (88009) - Rgb Light
    - [X] Two Port Hub (88009) - Current
    - [X] Two Port Hub (88009) - Voltage
    - [X] Mario Hub (set 71360) - Accelerometer (Raw & Gesture) (⚠ Usable but Gesture mapping is a rough draft)
    - [X] Mario Hub (set 71360) - TagSensor (Barcode & RGB)
    - [X] Mario Hub (set 71360) - Pants
    - [ ] Mario Hub (set 71360) - Debug
    - [X] Duplo Train Base (set 10874) - Motor
    - [X] Duplo Train Base (set 10874) - Speaker
    - [X] Duplo Train Base (set 10874) - Rgb Light
    - [X] Duplo Train Base (set 10874) - ColorSensor
    - [X] Duplo Train Base (set 10874) - Speedometer
    - [X] Color Distance Sensor (88007) (⚠ v4.0 or later)
    - [X] Medium Linear Motor (88008)
    - [X] Remote Control Button (88010)
    - [X] Remote Control RSSI (88010)
    - [X] Train Motor (88011)
    - [X] Technic Large Motor (88013)
    - [X] Technic XLarge Motor (88014)
    - [ ] Technic Medium Angular Motor (Spike)
    - [X] Technic Medium Angular Motor (Grey)
    - [ ] Technic Large Angular Motor (Spike)
    - [X] Technic Large Angular Motor (Grey)
    - [X] Technic Color Sensor
    - [X] Technic Distance Sensor
    - .. other devices depend on availability of hardware / contributions
- Protocol
  - [X] Message Encoding (98% [spec coverage](docs/specification/coverage.md))
  - [X] Knowledge
- Features
  - [X] Dynamic Device
  - [X] Deployment Verifier
- Command Line (`dotnet tool install -g SharpBrick.PoweredUp.Cli`)
  - [X] `poweredup device list` (discover all connected devices and their port (mode) properties)
  - [X] `poweredup device dump-static-port -p <port number>` (see [adding new devices tutorial](docs/development/adding-new-device.md))

## SDKs in other programming languages

- JavaScript (Node + Browser): 
  - [nathankellenicki/node-poweredup](https://github.com/nathankellenicki/node-poweredup)
- .NET / C#
  - **sharpbrick/powered-up** (this here)
  - [Vouzamo/Lego](https://github.com/Vouzamo/Lego) (and [blog](https://vouzamo.wordpress.com/2020/04/21/lego-c-sdk-enhancements-challenges/))
  - [Cosmik42/BAP](https://github.com/Cosmik42/BAP) (Lego Train Project ... Contains logic for the LWP)
- C++
  -  [corneliusmunz/legoino](https://github.com/corneliusmunz/legoino) (Arduino)
- Python
  - [alexsdutton/python-lego-wireless-protocol](https://github.com/alexsdutton/python-lego-wireless-protocol)
  - [undera/pylgbst](https://github.com/undera/pylgbst)
  - [virantha/bricknil](https://github.com/virantha/bricknil/)

## Resources

- [Lego Wireless Protocol Specification](https://lego.github.io/lego-ble-wireless-protocol-docs) ([GitHub](https://github.com/lego/lego-ble-wireless-protocol-docs))
- [BlueGiga BLE protcol](https://www.silabs.com/documents/public/reference-manuals/Bluetooth-LE-Software-API%20Reference-Manual-for-BLE-Version-1.10.pdf)

## Contribution

SharpBrick is an organization intended to host volunteers willing to contribute to the SharpBrick.PoweredUp and related projects. Everyone is welcome (private and commercial entities). Please read our **[Code of Conduct](CODE_OF_CONDUCT.md)** before participating in our project.

The product is licensed under **MIT License** to allow a easy and wide adoption into prviate and commercial products.

## Thanks ...

Thanks to [@nathankellenicki](https://github.com/nathankellenicki), [@dlech](https://github.com/dlech), [@corneliusmunz](https://github.com/corneliusmunz), [@KeyDecoder](https://github.com/KeyDecoder), [@highstreeto](https://github.com/highstreeto), [@Berdsen ](https://github.com/Berdsen), [@vuurbeving](https://github.com/vuurbeving) and [@dkurok](https://github.com/dkurok) for their code, answers, testing and other important contributions.
