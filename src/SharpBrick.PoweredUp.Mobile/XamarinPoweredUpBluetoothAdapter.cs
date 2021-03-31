using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        private readonly IAdapter _bluetoothAdapter;
        private readonly INativeDeviceInfoProvider _deviceInfoProvider;
        private readonly Dictionary<ulong, IDevice> _discoveredDevices = new Dictionary<ulong, IDevice>();

        public XamarinPoweredUpBluetoothAdapter(IBluetoothLE bluetooth, INativeDeviceInfoProvider deviceInfoProvider)
        {
            _bluetoothAdapter = bluetooth.Adapter;
            _deviceInfoProvider = deviceInfoProvider;
        }

        public void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            _bluetoothAdapter.ScanMode = ScanMode.Balanced;

            _bluetoothAdapter.DeviceDiscovered += ReceivedHandler;

            cancellationToken.Register(async () =>
            {
                await _bluetoothAdapter.StopScanningForDevicesAsync().ConfigureAwait(false);
                _bluetoothAdapter.DeviceDiscovered -= ReceivedHandler;
            });

            _bluetoothAdapter.StartScanningForDevicesAsync(new Guid[] { new Guid(PoweredUpBluetoothConstants.LegoHubService) }, DeviceFilter, false).ConfigureAwait(false);

            async void ReceivedHandler(object sender, DeviceEventArgs args)
            {
                var info = new PoweredUpBluetoothDeviceInfo();

                var advertisementRecord = args.Device.AdvertisementRecords.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);

                if (advertisementRecord?.Data?.Length > 0)
                {
                    var data = advertisementRecord.Data.ToList();
                    data.RemoveRange(0, 2);
                    info.ManufacturerData = data.ToArray();

                    info.Name = args.Device.Name;
                    info.BluetoothAddress = _deviceInfoProvider.GetNativeDeviceInfo(args.Device.NativeDevice).MacAddressNumeric;

                    AddInternalDevice(args.Device, info);
                    await discoveryHandler(info).ConfigureAwait(false);
                }
            }
        }

        private void AddInternalDevice(IDevice device, PoweredUpBluetoothDeviceInfo info)
        {
            if (!_discoveredDevices.ContainsKey(info.BluetoothAddress))
            {
                _discoveredDevices.Add(info.BluetoothAddress, device);
            }
            else
            {
                _discoveredDevices[info.BluetoothAddress] = device;
            }
        }

        private bool DeviceFilter(IDevice arg)
        {
            if (arg == null) 
            {
                return false;
            }

            var manufacturerData = arg.AdvertisementRecords.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);

            if (manufacturerData?.Data == null || manufacturerData.Data.Length < 8)
            {
                return false;
            }

            // https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#advertising
            // Length and Data Type Name seems to be already trimmed away
            // Manufacturer ID should be 0x0397 but seems in little endian encoding. I found no notice for this in the documentation except in version number encoding
            
            return manufacturerData.Data[0] == 0x97 && manufacturerData.Data[1] == 0x03;
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            if (!_discoveredDevices.ContainsKey(bluetoothAddress))
            {
                CancellationTokenSource cts = new CancellationTokenSource(10000);
                
                // trigger scan for 10 seconds
                Discover((deviceInfo) =>
                {
                    return Task.Run(() =>
                    {
                        cts.Cancel(false);
                    });
                    
                }, cts.Token);

                // 60 seconds will be ignored here, because the cancelation will happen after 10 seconds
                await Task.Delay(60000, cts.Token).ContinueWith(task => { });

                if (!_discoveredDevices.ContainsKey(bluetoothAddress))
                {
                    throw new NotSupportedException("Given bt address does not belong to a discovered device");
                }
            }

            return new XamarinPoweredUpBluetoothDevice(_discoveredDevices[bluetoothAddress], _bluetoothAdapter);
        }
    }

}
