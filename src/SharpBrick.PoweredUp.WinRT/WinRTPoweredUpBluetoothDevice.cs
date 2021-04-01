using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace SharpBrick.PoweredUp.WinRT
{
    public class WinRTPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private BluetoothLEDevice _device;
        public string Name => _device.Name;

        public WinRTPoweredUpBluetoothDevice(BluetoothLEDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        ~WinRTPoweredUpBluetoothDevice() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            _device?.Dispose();
            _device = null;
        }

        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            var gatt = await _device.GetGattServicesForUuidAsync(serviceId);

            if (gatt.Status == GattCommunicationStatus.Success && gatt.Services.Count > 0)
            {
                var service = gatt.Services[0];

                return new WinRTPoweredUpBluetoothService(service);
            }
            else
            {
                return null;
            }
        }
    }

}
