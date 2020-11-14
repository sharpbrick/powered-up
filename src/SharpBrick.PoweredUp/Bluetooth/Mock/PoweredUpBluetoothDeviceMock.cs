using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth.Mock
{
    public class PoweredUpBluetoothDeviceMock : IPoweredUpBluetoothDevice
    {
        private PoweredUpBluetoothServiceMock _mockService;

        public PoweredUpBluetoothDeviceMock(PoweredUpBluetoothServiceMock mockService)
        {
            this._mockService = mockService;
        }

        public string Name => "Mock Device";

        public void Dispose()
        { }

        public Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
            => Task.FromResult<IPoweredUpBluetoothService>(_mockService);
    }
}