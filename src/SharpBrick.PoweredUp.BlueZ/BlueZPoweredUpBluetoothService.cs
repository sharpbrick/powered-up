using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    internal class BlueZPoweredUpBluetoothService : IPoweredUpBluetoothService
    {
        private IGattService1 _gattService;

        public BlueZPoweredUpBluetoothService(IGattService1 gattService, Guid uuid)
        {
            Uuid = uuid;
            _gattService = gattService;
        }

        public Guid Uuid { get; }

        ~BlueZPoweredUpBluetoothService() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            _gattService = null;
        }

        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid characteristicId)
        {
            var characteristics = await Connection.System.FindProxies<IGattCharacteristic1>();

            foreach (var characteristic in characteristics)
            {
                var characteristicUuid = Guid.Parse(await characteristic.GetUUIDAsync());

                if (characteristicUuid == characteristicId)
                {
                    return new BlueZPoweredUpBluetoothCharacteristic(characteristic, characteristicId);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(characteristicId), $"Characteristic with id {characteristicId} not found");
            
        }
    }
}