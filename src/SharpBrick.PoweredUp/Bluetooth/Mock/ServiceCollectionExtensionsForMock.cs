using System;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Bluetooth.Mock;

namespace SharpBrick.PoweredUp;

public static class ServiceCollectionExtensionsForMock
{
    public static IServiceCollection AddMockBluetooth(this IServiceCollection self)
        => self.AddSingleton<IPoweredUpBluetoothAdapter, PoweredUpBluetoothAdapterMock>();

    public static PoweredUpBluetoothAdapterMock GetMockBluetoothAdapter(this IServiceProvider self)
        => self.GetService<IPoweredUpBluetoothAdapter>() as PoweredUpBluetoothAdapterMock;
}
