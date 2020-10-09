using HashtagChris.DotNetBlueZ;

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
        public Guid Uuid => Guid.Empty;

        private readonly GattCharacteristic _characteristic;

        public BlueZPoweredUpBluetoothCharacteristic(GattCharacteristic gattCharacteristic)
        {
            _characteristic = gattCharacteristic ?? throw new ArgumentNullException(nameof(gattCharacteristic));
        }

        public Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
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

            return Task.FromResult(true);
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            await _characteristic.WriteValueAsync(data, null);

            return true;
        }
    }
}
