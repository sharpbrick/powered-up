using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth.Mock;

public class PoweredUpBluetoothServiceMock : IPoweredUpBluetoothService
{
    private readonly PoweredUpBluetoothCharacteristicMock _mockCharacteristic;

    public PoweredUpBluetoothServiceMock(PoweredUpBluetoothCharacteristicMock mockCharacteristic)
    {
        _mockCharacteristic = mockCharacteristic;
    }

    public Guid Uuid => Guid.Empty;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        => Task.FromResult<IPoweredUpBluetoothCharacteristic>(_mockCharacteristic);
}
