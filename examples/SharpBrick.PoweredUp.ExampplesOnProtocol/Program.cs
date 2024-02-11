using SharpBrick.PoweredUp;

using Microsoft.Extensions.DependencyInjection; // SharpBrick.PoweredUp uses the DI system
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages; // SharpBrick.PoweredUp also logs stuff

var serviceProvider = new ServiceCollection()
    .AddLogging()
    .AddPoweredUp()
#if WINDOWS
    .AddWinRTBluetooth() // using WinRT Bluetooth on Windows (separate NuGet SharpBrick.PoweredUp.WinRT; others are available)
#endif
    .BuildServiceProvider();

// getting utilities
var bt = serviceProvider.GetService<IPoweredUpBluetoothAdapter>();
var host = serviceProvider.GetService<PoweredUpHost>();

// discover a LWP bluetooth device 
var tcs = new TaskCompletionSource<ILegoWirelessProtocol>();

bt.Discover(async deviceInfo =>
{
    if (!tcs.Task.IsCompleted)
    {
        var p = host.CreateProtocol(deviceInfo);

        tcs.SetResult(p);
    }
});

var protocol = await tcs.Task;

// connect the protocol
await protocol.ConnectAsync();

// send a raw message which should work with ANY motor connected to a hub
var response = await protocol.SendPortOutputCommandAsync(new PortOutputCommandStartPowerMessage(
    0, // PORT A
    PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
    100
)
{
    HubId = 0, // as if we ever see another one
});

await Task.Delay(2000);

await protocol.DisconnectAsync();