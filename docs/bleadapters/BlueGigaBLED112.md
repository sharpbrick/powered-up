# Using BlueGiga-adapter in powered-up
## Identify the name of the serial port ##

All tests in SharpBrick.Powered-Up have been done with a BlueGiga BLED112-adapter. This is an USB-Dongle which can be used on nearly all operating systems.

From the OS point of view a BlueGiga-adapter behaves like a standard SerialPort and is listed as such in the OS commands.

If you want to use a BlueGiga-adapter the first thing you've got to know is the name of the serial Port on which the adapter is attached to.
### Windows ###
On **Windows** you can find the name in the devicemanager (ControlPanel -> Device Manager) under the Ports (COM & LPT) section. There it is named "Bluegiga Bluetooth Low Energy (COMX)". The value in the round brackets is the name you've got to use, e.g. "COM4".

### Linux ###
On **Linux-based** OS (like we tested on Raspberry Pi 3 and 4 both runnning version 5.10.17 of the Raspian OS; use ````uname -a ```` to check for version; also tested on Ubuntu 20.04) there is ***no need*** to install additional software for the BlueGiga-adapter to work.  
You just need to identify the serial port, or in general the device, at which the adapter is attached. Use ````lsusb````
to list the USB-devices on your Linux-distro. The BlueGiga-adapter has an ID of 2458:0001. So here you know the adapter is attached.
Using ````lsmod```` you should find an entry "cdc_acm". If so you can list all devices adhering to the "abstract control model" (ACM)[^1] by using ````ls /dev/ttyACM*````. In case you've got more than one ttyACM-modules (in most cases these are USB-UART-devices) attached, you can use ````dmesg||grep tty```` to (hopefully) better identify the port-name of the adapter.  In most cases the name to use will be "/dev/ttyACM0". [^2]

## Using the BlueGiga in SharpBrick.PoweredUp-projects ##
If not yet installed, install the nuget-package `SharpBrick.PoweredUp.BlueGigaBLE` in your project.

Instead of using the WinRT-implementation (default) like this
````csharp
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT)
    .BuildServiceProvider();
````
you've got to use:
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
Instead of "COM4" use the name (e.g. "/dev/ttyACM0" for Linux-OS in most cases; "COMx" (x is a number) on Windows) you've identified above.  
For a full working example using commandline-parameters to switch to BlueGiga see [examples/SharpBrick.PoweredUp.Examples/Program.cs](../../examples/SharpBrick.PoweredUp.Examples/Program.cs) and 
 [examples/SharpBrick.PoweredUp.Examples/BaseExample.cs](../../examples/SharpBrick.PoweredUp.Examples/BaseExample.cs).
On windows-systems you can debug or run from inside Visual Studio / your IDE without any need for adaption in the build-process.

## Command Line Experience ##

````
   poweredup device list --usebluegiga COM4 --tracebluegiga
````
``--usebluegiga`` is the parameter that tells the cli-tool to use the BleuGiga-implementation. It has to be followed by the name of the serial port to use.
Use the name of the serial port as described above; so replace `COM4` with COMx whatever x is used on your Windows-system or e.g. /dev/ttyACM0 for Linux-based OS.   
`--tracebluegiga` is an optional parameter; when used the cli emits **a lot** of additional trace-information of the BleuGiga-implementation. Default: no additional trace.

## Using / Building for Linux-based OS ##
At the time of this writing you've got to make sure that you don't include/depend on the SharpBrick.PoweredUp.WinRT-project, because this doesn't build for Linux-platforms. So change your project-dependencies accordingly.
The following instructions assume that you've got installed at least the .NET 5 runtime on your Linux-based OS. See under Resources below for help on that.


### Build and run for Raspberry Pi ###
- open a command prompt on your dev-machine (Windows)
- cd to the directory of your project
- ``dotnet publish -r linux-arm -o bin\\linux-arm\\publish --no-self-contained``
- copy all content from bin\linux-arm\publish to a directory (e.g. powered-up under your home-directory) on your Raspberry Pi
- Change the mod of the executable (the file which is named like your project) to "executable" by using ``chmod +x (filename)`` on the Raspberry Pi
- Open a terminal-windows on the Pi and enter
  - ``cd ~/powered-up``
  - ``./SharpBrick.PoweredUp.Examples --usebluegiga /dev/ttyACM0``

### Build and run for Ubuntu 20.04 ###
- open a command prompt on your dev-machine (Windows)
- cd to the directory of your project
- ``dotnet publish -r linux-x64 -o bin\\linux-x64\\publish --no-self-contained``
- copy all content from bin\linux-x64\publish to a directory (e.g. powered-up under your home-directory) on your Ubuntu
- Change the mod of the executable (the file which is named like your project) to "executable" by using ``chmod +x (filename)`` on Ubuntu
- Open a terminal-windows on Ubuntu and enter
  - ``cd ~/powered-up``
  - ``./SharpBrick.PoweredUp.Examples --usebluegiga /dev/ttyACM0``
- In case you receive a permission-denied error for /dev/ttyACM0:
  - sudo chmod +rw /dev/ttyACM0
  - sudo chmod o+rw /dev/ttyACM0
  - sudo chmod g+rw /dev/ttyACM0

### Build and run for other Linux-OS ###
As far as there are .NET-runtimes available, you can follow the instructions above, but may need to use another runtime-identifier in the publish-command (so something different from 'linux-arm' or 'linux-x64'; depends on your distro). See below under Resources for help on that.


## Resources
- [BlueGiga BLE protcol](https://www.silabs.com/documents/public/reference-manuals/Bluetooth-LE-Software-API%20Reference-Manual-for-BLE-Version-1.10.pdf)
- [Install .NET 5 SDK on Raspberry Pi](https://www.petecodes.co.uk/install-and-use-microsoft-dot-net-5-with-the-raspberry-pi/)
- [Install .NET on Ubuntu 20.04](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-)
- [Other runtime-identifier for other Linux-distros](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

## Footnotes
[^1]: [What is the difference between /dev/ttyUSB and /dev/ttyACM?](https://rfc1149.net/blog/2013/03/05/what-is-the-difference-between-devttyusbx-and-devttyacmx/)
[^2]: [Find BLED112 on raspberry pi and check port-configuration](https://docs.rs-online.com/abcd/0900766b812eb651.pdf
) 

