using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth;

public interface IPoweredUpBluetoothService : IDisposable
{
    Guid Uuid { get; }
    Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid);
}
