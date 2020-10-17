using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothService : IPoweredUpBluetoothService
    {
        private readonly IGattService1 _service;

        public Guid Uuid => Guid.Parse(_service.GetUUIDAsync().Result);


        internal BlueZPoweredUpBluetoothService(IGattService1 service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            /*var characteristic = await _service.GetCharacteristicAsync(guid.ToString());
*/
            return Task.FromResult<IPoweredUpBluetoothCharacteristic>(new BlueZPoweredUpBluetoothCharacteristic(guid));
        }
    }
}
