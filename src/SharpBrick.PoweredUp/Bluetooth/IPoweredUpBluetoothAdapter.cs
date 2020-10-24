using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothAdapter
    {
        void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default);

        Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress);
    }
}