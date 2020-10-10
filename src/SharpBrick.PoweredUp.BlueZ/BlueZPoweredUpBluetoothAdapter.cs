using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.BlueZ.Utilities;

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
        private readonly BluetoothAddressFormatter _bluetoothAddressFormatter;

        public BlueZPoweredUpBluetoothAdapter(BluetoothAddressFormatter bluetoothAddressFormatter)
        {
            _adapter = BlueZManager.GetAdaptersAsync().Result.FirstOrDefault();
            _bluetoothAddressFormatter = bluetoothAddressFormatter;
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
                    info.ManufacturerData = (byte[])manufacturerData.First().Value;

                    info.Name = await eventArgs.Device.GetNameAsync();
                }

                var btAddress = await eventArgs.Device.GetAddressAsync();

                info.BluetoothAddress = _bluetoothAddressFormatter.ConvertToInteger(btAddress);

                discoveryHandler(info);
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var device = await _adapter.GetDeviceAsync(_bluetoothAddressFormatter.ConvertToMacString(bluetoothAddress));

            return new BlueZPoweredUpBluetoothDevice(device);
        }
    }
}
