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

# Examples

Additional to code fragments below, look into the `examples/SharpBrick.PoweredUp.Examples` project (15+ examples).

````csharp
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.WinRT; // for WinRT Bluetooth NuGet
````

## Discovering Hubs

````csharp
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows
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
    // optionally verify if everything is wired up correctly (v2.0 onwards)
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
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows
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
  - [X] .NET Core 3.1 (on Windows 10 using WinRT)
    - Library uses `Span<T>` / C# 8.0 and is therefore not supported in .NET Framework 1.0 - 4.8 and UWP Apps until arrival of .NET 5 (WinForms and WPF work in .NET Core 3.1)
    - Library uses WinRT for communication therefore only Windows 10
  - [ ] Xamarin (on iOS / Android using ?)
  - [ ] Blazor (on Browser using WebBluetooth)
- Hub Model
  - Hubs
    - [X] Ports
    - [X] Properties
    - [X] Alerts
    - [X] Actions
    - [X] Create Virtual Ports
    - [X] Technic Medium Hub
    - [X] Two Port Hub (88009)
    - .. other hubs depend on availability of hardware / contributions
  - Devices
    - [X] Technic Medium Hub - Rgb Light
    - [X] Technic Medium Hub - Current
    - [X] Technic Medium Hub - Voltage
    - [X] Technic Medium Hub - Temperature Sensor 1 + 2
    - [X] Technic Medium Hub - Accelerometer
    - [X] Technic Medium Hub - Gyro Sensor
    - [X] Technic Medium Hub - Tilt Sensor
    - [X] Technic Medium Hub - Gesture Sensor (⚠ Usable but Gesture mapping is pending)
    - [X] Technic XLarge Motor
    - [X] Technic Large Motor
    - [ ] Technic Angular Motor (depend on availability of hardware / contributions)
    - [X] Hub (88009) - Rgb Light
    - [X] Hub (88009) - Current
    - [X] Hub (88009) - Voltage
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

## Contribution

SharpBrick is an organization intended to host volunteers willing to contribute to the SharpBrick.PoweredUp and related projects. Everyone is welcome (private and commercial entities). Please read our **[Code of Conduct](CODE_OF_CONDUCT.md)** before participating in our project.

The product is licensed under **MIT License** to allow a easy and wide adoption into prviate and commercial products.

## Thanks ...

Thanks to @nathankellenicki and @corneliusmunz for their code, answers, testing and other important contributions.