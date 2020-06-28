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
    - Library uses `Span<T>` / C# 8.0 and is therefore not supported in .NET Framework 1.0 - 4.8 and UWP Apps until arrival of .NET 5 (WinForms and WPF work in .NET Core 3.1)
    - Library uses WinRT for communication therefore only Windows 10
  - [ ] Xamarin (on iOS / Android using Xamarin.Essentials)
  - [ ] Blazor (on Browser using WebBluetooth)
- Hub Model
  - Hubs
    - [X] Ports
    - [X] Properties
    - [X] Alerts
    - [X] Actions
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
    - [ ] Technic Angular Motor (depend on availability of hardware / contributions)
- Protocol
  - [X] Message Encoding (95% [spec coverage](docs/specification/coverage.md))
  - [X] Knowledge
- Command Line (`dotnet install -g SharpBrick.PoweredUp.Cli`)
  - [X] `poweredup device list` (discover all connected devices and their port (mode) properties)
  - [X] `poweredup device dump-static-port -p <port number>` (see [adding new devices tutorial](docs/development/adding-new-device.md))

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
