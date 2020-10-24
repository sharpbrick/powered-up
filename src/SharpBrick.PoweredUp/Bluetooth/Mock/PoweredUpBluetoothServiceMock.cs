using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth.Mock
{
    public class PoweredUpBluetoothServiceMock : IPoweredUpBluetoothService
    {
        private PoweredUpBluetoothCharacteristicMock _mockCharacteristic;

        public PoweredUpBluetoothServiceMock(PoweredUpBluetoothCharacteristicMock mockCharacteristic)
        {
            this._mockCharacteristic = mockCharacteristic;
        }

        public Guid Uuid => Guid.Empty;

        public void Dispose()
        {
        }

        public Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
            => Task.FromResult<IPoweredUpBluetoothCharacteristic>(_mockCharacteristic);
    }
}