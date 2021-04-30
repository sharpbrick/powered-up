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
        public void Discover(Func<IPoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
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
                var info = new PoweredUpBluetoothDeviceInfoWithMacAddress();

                if (eventArgs.Advertisement.ManufacturerData.Count > 0)
                {
                    var reader = DataReader.FromBuffer(eventArgs.Advertisement.ManufacturerData[0].Data);
                    var data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    info.ManufacturerData = data;

                    using var device = BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress).AsTask().Result;

                    info.Name = device.Name;
                }

                info.MacAddressAsUInt64 = eventArgs.BluetoothAddress;

                await discoveryHandler(info);
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(IPoweredUpBluetoothDeviceInfo bluetoothDeviceInfo)
        {
            var bluetoothAddress = (bluetoothDeviceInfo is PoweredUpBluetoothDeviceInfoWithMacAddress local) ? local.MacAddressAsUInt64 : throw new ArgumentException("DeviceInfo not created by adapter", nameof(bluetoothDeviceInfo));

            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            return new WinRTPoweredUpBluetoothDevice(device);
        }


        public Task<IPoweredUpBluetoothDeviceInfo> CreateDeviceInfoByKnownStateAsync(object state)
            => Task.FromResult<IPoweredUpBluetoothDeviceInfo>(state switch
            {
                ulong address => new PoweredUpBluetoothDeviceInfoWithMacAddress() { MacAddressAsUInt64 = address },
                _ => null,
            });
    }
}
