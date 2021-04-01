using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace SharpBrick.PoweredUp.WinRT
{
    public class WinRTPoweredUpBluetoothService : IPoweredUpBluetoothService
    {
        private GattDeviceService _service;
        public Guid Uuid => _service.Uuid;

        public WinRTPoweredUpBluetoothService(GattDeviceService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        ~WinRTPoweredUpBluetoothService() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        {
            _service.Dispose();
            _service = null;
        }

        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            var characteristics = await _service.GetCharacteristicsForUuidAsync(guid);

            if (characteristics.Status == GattCommunicationStatus.Success && characteristics.Characteristics.Count > 0)
            {
                var characteristic = characteristics.Characteristics[0];

                return new WinRTPoweredUpBluetoothCharacteristic(characteristic);
            }
            else
            {
                return null;
            }
        }
    }

}
