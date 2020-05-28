using System;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace SharpBrick.PoweredUp.WinRT
{
    public class WinRTPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        public void Discover(Action<PoweredUpBluetoothDeviceInfo> discoveryHandler, CancellationToken cancellationToken = default)
        {
            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(new Guid(PoweredUpBluetoothConstants.LegoHubService));

            watcher.Received += ReceivedHandler;

            cancellationToken.Register(() =>
            {
                watcher.Stop();
                watcher.Received -= ReceivedHandler;
            });

            watcher.Start();

            void ReceivedHandler(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                var info = new PoweredUpBluetoothDeviceInfo();

                info.BluetoothAddress = eventArgs.BluetoothAddress;

                discoveryHandler(info);
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            return new WinRTPoweredUpBluetoothDevice(device);
        }
    }

}
