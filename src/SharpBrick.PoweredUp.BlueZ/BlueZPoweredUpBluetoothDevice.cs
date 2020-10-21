using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    internal class BlueZPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private Func<PoweredUpBluetoothDeviceInfo, Task> _discoveryHandler;
        private IDevice1 _device;

        internal PoweredUpBluetoothDeviceInfo DeviceInfo { get; private set; } = new PoweredUpBluetoothDeviceInfo();
        internal bool Connected { get; private set; } = false;
        internal bool ServicesResolved { get; private set;} = false;

        internal BlueZPoweredUpBluetoothDevice(IDevice1 device, Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler = null)
        {
            _discoveryHandler = discoveryHandler;
            _device = device;
        }

        public string Name { get; private set; } = string.Empty;

        private async Task InvokeDiscoveryHandlerAsync()
        {
            if (_discoveryHandler != null && DeviceInfo?.ManufacturerData != null)
            {
                var tempHandle = _discoveryHandler;
                _discoveryHandler = null; // make sure we only execute the handler exactly once.
                await tempHandle(DeviceInfo);
            }
        }

        internal async Task Initialize()
        {
            await _device.WatchPropertiesAsync(DevicePropertyChangedHandler);

            await GetSafeDeviceInfoAsync();
        }

        internal async Task TryGetManufacturerDataAsync()
        {
            try 
            {
                var manufacturerData = await _device.GetManufacturerDataAsync();
                DeviceInfo.ManufacturerData = (byte[])manufacturerData.First().Value;
            } 
            catch 
            {
                // we can ignore errors here, this will throw an exception for existing devices from a previous session (only after reboot/restart of bluetoothd)
                // for these device, manufacturer data will be returned when discovery is turned on
            }
        }

        private async void DevicePropertyChangedHandler(PropertyChanges changes)
        {
            foreach (var propertyChanged in changes.Changed)
            {
                switch (propertyChanged.Key)
                {
                    case "ManufacturerData":
                        DeviceInfo.ManufacturerData = (byte[])((IDictionary<ushort,object>)propertyChanged.Value).First().Value;
                        break;
                    case "Connected":
                        Connected = (bool)propertyChanged.Value;
                        break;
                    case "ServicesResolved":
                        ServicesResolved = (bool)propertyChanged.Value;
                        break;
                    case "RSSI":
                        // this is the only dependable property that will change from a pre-existing device during discovery
                        await InvokeDiscoveryHandlerAsync();
                        break;
                }
            }
        }

        internal async Task GetSafeDeviceInfoAsync()
        {
            var btAddress = await _device.GetAddressAsync();
            DeviceInfo.BluetoothAddress = Utilities.BluetoothAddressFormatter.ConvertToInteger(btAddress);
            DeviceInfo.Name = Name = await _device.GetNameAsync();
        }

        ~BlueZPoweredUpBluetoothDevice() => Dispose(false);
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual async void Dispose(bool disposing)
        {
            if (Connected)
                await _device?.DisconnectAsync(); // dangerous to await here, try to find a better way

            _device = null;
        }

        private async Task WaitForConnectionAndServicesResolved(CancellationToken token)
        {
            while (!Connected || !ServicesResolved) 
            { 
                token.ThrowIfCancellationRequested();
                await Task.Delay(25, token); 
            }
        }

        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            var connectionTimeout = TimeSpan.FromSeconds(5);

            var cancellationTokenSource = new CancellationTokenSource();

            await _device.ConnectAsync();

            cancellationTokenSource.CancelAfter(connectionTimeout);

            await WaitForConnectionAndServicesResolved(cancellationTokenSource.Token);

            var gattServices = await Connection.System.FindProxies<IGattService1>();

            foreach (var gattService in gattServices)
            {
                var gattUuid = Guid.Parse(await gattService.GetUUIDAsync());

                if (gattUuid == serviceId) 
                {
                    return new BlueZPoweredUpBluetoothService(gattService, gattUuid);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(serviceId), $"Service with id {serviceId} not found");
        }
    }
}