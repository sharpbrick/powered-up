using Polly;

using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothCharacteristic : IPoweredUpBluetoothCharacteristic
    {
        public Guid Uuid => Guid.Parse(_characteristic.GetUUIDAsync().Result);

        private readonly IGattCharacteristic1 _characteristic;

        public BlueZPoweredUpBluetoothCharacteristic(Guid characteristicUuid)
        {
            //_characteristic = gattCharacteristic ?? throw new ArgumentNullException(nameof(gattCharacteristic));
        }

        public Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            // if (notificationHandler is null)
            // {
            //     throw new ArgumentNullException(nameof(notificationHandler));
            // }

            // _characteristic.Value += ValueChangedHandler;

            // Task ValueChangedHandler(GattCharacteristic sender, GattCharacteristicValueEventArgs args)
            // {
            //     return notificationHandler(args.Value);
            // }

            // //await _characteristic.StartNotifyAsync();
            
            return Task.FromResult(true);
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            await _characteristic.WriteValueAsync(data, new Dictionary<string,object>());

            await Policy
              .Handle<Tmds.DBus.DBusException>()
              .WaitAndRetryForeverAsync(_ => TimeSpan.FromMilliseconds(100))
              .ExecuteAsync(() => _characteristic.WriteValueAsync(data, new Dictionary<string, object>()));


            return true;
        }
    }
}
