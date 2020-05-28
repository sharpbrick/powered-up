using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothDevice : IDisposable
    {
        string Name { get; }
        Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId);
    }
}