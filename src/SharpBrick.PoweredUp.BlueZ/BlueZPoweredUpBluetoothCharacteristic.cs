using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        internal BlueZPoweredUpBluetoothCharacteristic(IGattCharacteristic1 characteristic, ILogger logger)
        {
            _logger = logger;
            _characteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {

            if (notificationHandler is null)
            {
                 throw new ArgumentNullException(nameof(notificationHandler));
            }

            await _characteristic.WatchPropertiesAsync(PropertyChangedHandler);

            void PropertyChangedHandler(PropertyChanges changes)
            {
                foreach (var changed in changes.Changed)
                {
                    if (changed.Key == "Value")
                    {
                        notificationHandler((byte[])changed.Value);
                    }
                }
            }

            await _characteristic.StartNotifyAsync();
            
            return true;
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            //await _characteristic.WriteValueAsync(data, new Dictionary<string,object>());

            await Policy
              .Handle<Tmds.DBus.DBusException>()
              .WaitAndRetryForeverAsync(_ => TimeSpan.FromMilliseconds(100))
              .ExecuteAsync(() => _characteristic.WriteValueAsync(data, new Dictionary<string, object>()));


            return true;
        }
    }
}
