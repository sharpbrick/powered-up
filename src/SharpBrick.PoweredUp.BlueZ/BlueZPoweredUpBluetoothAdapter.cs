using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        private readonly Adapter _adapter;

        public BlueZPoweredUpBluetoothAdapter()
        {
            _adapter = BlueZManager.GetAdaptersAsync().Result.FirstOrDefault();
        }

        public async void Discover(Action<PoweredUpBluetoothDeviceInfo> discoveryHandler, CancellationToken cancellationToken = default)
        {
            _adapter.DeviceFound += DeviceFoundHandler;

            cancellationToken.Register(async () =>
            {
                await _adapter.StopDiscoveryAsync();
                _adapter.DeviceFound -= DeviceFoundHandler;
            });

            await _adapter.StartDiscoveryAsync();

            async Task DeviceFoundHandler(Adapter sender, DeviceFoundEventArgs eventArgs)
            {
                var info = new PoweredUpBluetoothDeviceInfo();

                var manufacturerData = await eventArgs.Device.GetManufacturerDataAsync();

                if (manufacturerData.Count > 0)
                {

                    info.ManufacturerData = manufacturerData[0] as byte[];

                    info.Name = await eventArgs.Device.GetNameAsync();
                }

                info.BluetoothAddress = ulong.Parse(await eventArgs.Device.GetAddressAsync());

                discoveryHandler(info);
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var device = await _adapter.GetDeviceAsync(bluetoothAddress.ToString());

            return new BlueZPoweredUpBluetoothDevice(device);
        }
    }
}
