using Microsoft.Extensions.DependencyInjection;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public static class IServiceCollectionExtensionsForXamarin
    {
        public static IServiceCollection AddXamarinBluetooth(this IServiceCollection self, INativeDeviceInfo deviceInfo)
            => self
               .AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current)
               .AddSingleton<INativeDeviceInfo>(deviceInfo)
               .AddSingleton<IPoweredUpBluetoothAdapter, XamarinPoweredUpBluetoothAdapter>();
    }
}
