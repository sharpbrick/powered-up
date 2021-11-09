using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.WinRT;

namespace SharpBrick.PoweredUp;

public static class ServiceCollectionExtensionsForWinRT
{
    public static IServiceCollection AddWinRTBluetooth(this IServiceCollection self)
        => self.AddSingleton<IPoweredUpBluetoothAdapter, WinRTPoweredUpBluetoothAdapter>();
}
