using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

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


        public BlueZPoweredUpBluetoothService(IGattService1 gattService)
        {
            _service = gattService ?? throw new ArgumentNullException(nameof(gattService));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            var characteristic = await _service.GetCharacteristicAsync(guid.ToString());

            return new BlueZPoweredUpBluetoothCharacteristic(characteristic);
        }
    }
}
