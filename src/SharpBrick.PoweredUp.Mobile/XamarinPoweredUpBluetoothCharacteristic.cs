using System;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinPoweredUpBluetoothCharacteristic : IPoweredUpBluetoothCharacteristic
    {
        private readonly ICharacteristic _characteristic;

        public Guid Uuid => _characteristic.Id;

        public XamarinPoweredUpBluetoothCharacteristic(ICharacteristic characteristic)
        {
            _characteristic = characteristic;
        }

        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            if (notificationHandler is null)
            {
                throw new ArgumentNullException(nameof(notificationHandler));
            }

            _characteristic.ValueUpdated += ValueUpdatedHandler;

            void ValueUpdatedHandler(object sender, CharacteristicUpdatedEventArgs e)
            {
                notificationHandler(e.Characteristic.Value);
            }

            await _characteristic.StartUpdatesAsync();

            return true;
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return await _characteristic.WriteAsync(data);
        }
    }
}
