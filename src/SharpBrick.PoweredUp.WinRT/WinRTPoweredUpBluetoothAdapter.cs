using System;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace SharpBrick.PoweredUp.WinRT
{
    public class WinRTPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        public void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(new Guid(PoweredUpBluetoothConstants.LegoHubService));

            watcher.Received += ReceivedHandler;

            cancellationToken.Register(() =>
            {
                watcher.Stop();
                watcher.Received -= ReceivedHandler;
            });

            watcher.Start();

            async void ReceivedHandler(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                var info = new PoweredUpBluetoothDeviceInfo();

                if (eventArgs.Advertisement.ManufacturerData.Count > 0)
                {
                    var reader = DataReader.FromBuffer(eventArgs.Advertisement.ManufacturerData[0].Data);
                    var data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    info.ManufacturerData = data;

                    using var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result;

                    info.Name = device.Name;
                }

                info.BluetoothAddress = eventArgs.BluetoothAddress;

                await discoveryHandler(info);
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            return new WinRTPoweredUpBluetoothDevice(device);
        }
    }

}
