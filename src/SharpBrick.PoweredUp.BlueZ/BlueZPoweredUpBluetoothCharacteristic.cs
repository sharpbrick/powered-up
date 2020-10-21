using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using SharpBrick.PoweredUp.Bluetooth;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    internal class BlueZPoweredUpBluetoothCharacteristic : IPoweredUpBluetoothCharacteristic
    {
        private IGattCharacteristic1 _characteristic;

        public BlueZPoweredUpBluetoothCharacteristic(IGattCharacteristic1 characteristic, Guid uuid)
        {
            Uuid = uuid;
            _characteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        public Guid Uuid { get; } 

        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            if (notificationHandler is null)
            {
                 throw new ArgumentNullException(nameof(notificationHandler));
            }

            await _characteristic.WatchPropertiesAsync(PropertyChangedHandler);

            await _characteristic.StartNotifyAsync();
            
            return true;

            void PropertyChangedHandler(PropertyChanges propertyChanges)
            {
                foreach (var propertyChanged in propertyChanges.Changed)
                {
                    if (propertyChanged.Key == "Value")
                    {
                        notificationHandler((byte[])propertyChanged.Value);
                    }
                }
            }
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            await Policy
              .Handle<Tmds.DBus.DBusException>()
              .WaitAndRetryForeverAsync(_ => TimeSpan.FromMilliseconds(10))
              .ExecuteAsync(() => _characteristic.WriteValueAsync(data, new Dictionary<string, object>()));

            return true;
        }
    }
}