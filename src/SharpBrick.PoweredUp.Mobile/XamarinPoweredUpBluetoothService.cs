using System;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinPoweredUpBluetoothService : IPoweredUpBluetoothService
    {
        private IService _service;

        public Guid Uuid => _service.Id;

        public XamarinPoweredUpBluetoothService(IService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        #region IDisposable

        ~XamarinPoweredUpBluetoothService() => Dispose(false);

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            _service.Dispose();
            _service = null;
        }

        #endregion

        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            var characteristic = await _service.GetCharacteristicAsync(guid);

            if (characteristic == null) return null;

            // await characteristic.StartUpdatesAsync();
            return new XamarinPoweredUpBluetoothCharacteristic(characteristic);

        }
    }
}
