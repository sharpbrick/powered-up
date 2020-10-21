using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.BlueZ;
using SharpBrick.PoweredUp.BlueZ.Utilities;

namespace SharpBrick.PoweredUp
{
    public static class ServiceCollectionExtensionsForBlueZ
    {
        public static IServiceCollection AddBlueZBluetooth(this IServiceCollection self)
            => self.AddSingleton<IPoweredUpBluetoothAdapter, BlueZPoweredUpBluetoothAdapter>();
    }
}