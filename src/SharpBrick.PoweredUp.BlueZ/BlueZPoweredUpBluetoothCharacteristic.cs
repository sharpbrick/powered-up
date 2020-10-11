using HashtagChris.DotNetBlueZ;

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

        private readonly GattCharacteristic _characteristic;

        public BlueZPoweredUpBluetoothCharacteristic(GattCharacteristic gattCharacteristic)
        {
            _characteristic = gattCharacteristic ?? throw new ArgumentNullException(nameof(gattCharacteristic));
        }

        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            if (notificationHandler is null)
            {
                throw new ArgumentNullException(nameof(notificationHandler));
            }

            _characteristic.Value += ValueChangedHandler;

            Task ValueChangedHandler(GattCharacteristic sender, GattCharacteristicValueEventArgs args)
            {
                return notificationHandler(args.Value);
            }

            //await _characteristic.StartNotifyAsync();
            
            return true;
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
