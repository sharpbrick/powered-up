using Microsoft.Extensions.DependencyInjection;

using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.BlueZ.Utilities;

namespace SharpBrick.PoweredUp.BlueZ
{
    public static class ServiceCollectionExtensionsForBlueZ
    {
        public static IServiceCollection AddBlueZBluetooth(this IServiceCollection self)
        {
            self.AddSingleton<BluetoothAddressFormatter>();
            return self.AddSingleton<IPoweredUpBluetoothAdapter, BlueZPoweredUpBluetoothAdapter>();
        }
    }
}
