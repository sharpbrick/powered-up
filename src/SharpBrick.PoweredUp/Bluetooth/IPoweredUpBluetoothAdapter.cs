using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothAdapter
    {
        void Discover(Action<PoweredUpBluetoothDeviceInfo> discoveryHandler, CancellationToken cancellationToken = default);

        Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress);
    }
}