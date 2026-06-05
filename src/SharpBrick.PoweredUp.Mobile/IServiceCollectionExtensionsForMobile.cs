using Microsoft.Extensions.DependencyInjection;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public static class IServiceCollectionExtensionsForMobile
    {
        public static IServiceCollection AddMobileBluetooth(this IServiceCollection self, INativeDeviceInfoProvider deviceInfoProvider)
            => self
               .AddSingleton<IBluetoothLE>(CrossBluetoothLE.Current)
               .AddSingleton<INativeDeviceInfoProvider>(deviceInfoProvider)
               .AddSingleton<IPoweredUpBluetoothAdapter, MobilePoweredUpBluetoothAdapter>();
    }
}
