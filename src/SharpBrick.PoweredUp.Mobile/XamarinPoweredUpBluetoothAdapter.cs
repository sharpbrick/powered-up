using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Xamarin
{
    public class XamarinPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        private IAdapter bluetoothAdapter;

        public XamarinPoweredUpBluetoothAdapter(IBluetoothLE bluetooth)
        {
            bluetoothAdapter = bluetooth.Adapter;
        }

        public void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            bluetoothAdapter.ScanMode = ScanMode.Balanced;

            bluetoothAdapter.DeviceDiscovered += ReceivedHandler;

            cancellationToken.Register(async () =>
            {
                await bluetoothAdapter.StopScanningForDevicesAsync().ConfigureAwait(false);
                bluetoothAdapter.DeviceDiscovered -= ReceivedHandler;
            });

            bluetoothAdapter.StartScanningForDevicesAsync(new Guid[] { new Guid(PoweredUpBluetoothConstants.LegoHubService) }, DeviceFilter, false).ConfigureAwait(false);

            async void ReceivedHandler(object sender, DeviceEventArgs args)
            {
                var info = new PoweredUpBluetoothDeviceInfo();

                var advertisementRecord = args.Device.AdvertisementRecords.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);

                if (advertisementRecord?.Data?.Length > 0)
                {
                    info.ManufacturerData = advertisementRecord.Data;
                    info.Name = args.Device.Name;
                    // info.BluetoothAddress = eventArgs.BluetoothAddress;

                    await discoveryHandler(info).ConfigureAwait(false);
                }


            }
        }

        private bool DeviceFilter(IDevice arg)
        {
            if (arg == null) return false;

            System.Diagnostics.Debug.WriteLine(arg.Name);

            var manufacturerData = arg.AdvertisementRecords.FirstOrDefault(x => x.Type == AdvertisementRecordType.ManufacturerSpecificData);

            if (manufacturerData?.Data == null || manufacturerData.Data.Length < 8) return false;

            // https://lego.github.io/lego-ble-wireless-protocol-docs/index.html#advertising
            // Length and Data Type Name seems to be already trimmed away
            // Manufacturer ID should be 0x0397 but seems in little endian encoding. I found no notice for this in the documentation except in version number encoding

            switch (manufacturerData.Data[3])
            {
                case 0x00:
                    System.Diagnostics.Debug.WriteLine("System: LEGO Wedo 2.0, Device: WeDo Hub");
                    break;
                case 0x20:
                    System.Diagnostics.Debug.WriteLine("System: LEGO Duplo, Device: Duplo Train");
                    break;
                case 0x40:
                    System.Diagnostics.Debug.WriteLine("System: System, Device: Boost Hub");
                    break;
                case 0x41:
                    System.Diagnostics.Debug.WriteLine("System: System, Device: 2 Port Hub");
                    break;
                case 0x42:
                    System.Diagnostics.Debug.WriteLine("System: System, Device: 2 Port Handset");
                    break;
                default:
                    if (manufacturerData.Data[3] >= 96 && manufacturerData.Data[3] < 128)
                    {
                        System.Diagnostics.Debug.WriteLine("System: LEGO System, Device: Currently unknown");
                    }
                    break;
            }

            return manufacturerData.Data[0] == 0x97 || manufacturerData.Data[1] == 0x03;
        }

        public Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            // var device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
            // return new WinRTPoweredUpBluetoothDevice(device);

            throw new NotImplementedException();
        }
    }

}
