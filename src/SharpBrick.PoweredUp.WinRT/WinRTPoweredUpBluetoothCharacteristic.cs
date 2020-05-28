using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace SharpBrick.PoweredUp.WinRT
{

    public class WinRTPoweredUpBluetoothCharacteristic : IPoweredUpBluetoothCharacteristic
    {
        private GattCharacteristic _characteristic;
        public Guid Uuid => _characteristic.Uuid;

        public WinRTPoweredUpBluetoothCharacteristic(GattCharacteristic characteristic)
        {
            _characteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            if (notificationHandler is null)
            {
                throw new ArgumentNullException(nameof(notificationHandler));
            }

            _characteristic.ValueChanged += ValueChangedHandler;

            void ValueChangedHandler(GattCharacteristic sender, GattValueChangedEventArgs args)
            {
                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                var data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);

                notificationHandler(data);
            }

            var status = await _characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            return (status == GattCommunicationStatus.Success);
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var writer = new DataWriter();
            writer.WriteBytes(data);

            var writeResult = await _characteristic.WriteValueWithResultAsync(writer.DetachBuffer());

            return (writeResult.Status == GattCommunicationStatus.Success);
        }
    }

}
