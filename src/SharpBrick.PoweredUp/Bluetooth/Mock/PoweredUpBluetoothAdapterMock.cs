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

        public Task<IPoweredUpBluetoothDeviceInfo> CreateDeviceInfoByKnownStateAsync(object state)
        {
            return Task.FromResult<IPoweredUpBluetoothDeviceInfo>(new PoweredUpBluetoothDeviceInfoWithMacAddress());
        }

        public void Discover(Func<IPoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPoweredUpBluetoothDevice> GetDeviceAsync(IPoweredUpBluetoothDeviceInfo bluetoothDeviceInfo)
        {
            return Task.FromResult<IPoweredUpBluetoothDevice>(MockDevice);
        }
    }
}