using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth.Mock
{
    public class PoweredUpBluetoothAdapterMock : IPoweredUpBluetoothAdapter
    {
        public PoweredUpBluetoothAdapterMock()
        {
            MockCharacteristic = new PoweredUpBluetoothCharacteristicMock();
            MockService = new PoweredUpBluetoothServiceMock(MockCharacteristic);
            MockDevice = new PoweredUpBluetoothDeviceMock(MockService);
        }

        public PoweredUpBluetoothDeviceMock MockDevice { get; }
        public PoweredUpBluetoothServiceMock MockService { get; }
        public PoweredUpBluetoothCharacteristicMock MockCharacteristic { get; }

        public void Discover(Action<PoweredUpBluetoothDeviceInfo> discoveryHandler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            return Task.FromResult<IPoweredUpBluetoothDevice>(MockDevice);
        }
    }
}