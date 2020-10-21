using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.BlueZ.Utilities;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        private readonly ILogger<BlueZPoweredUpBluetoothAdapter> _logger;
        private readonly string _adapterObjectPath;
        private readonly Dictionary<ulong, IPoweredUpBluetoothDevice> _devices = new Dictionary<ulong, IPoweredUpBluetoothDevice>();
        private IAdapter1 _adapter;

        public bool Discovering { get; set; } = false;

        public BlueZPoweredUpBluetoothAdapter( 
            ILogger<BlueZPoweredUpBluetoothAdapter> logger,
            string adapterObjectPath = null) //"/org/bluez/hci0")
        {
            _logger = logger;
            _adapterObjectPath = adapterObjectPath;
        }

        private async Task<IAdapter1> GetAdapterAsync()
        {
            var adapter = !string.IsNullOrEmpty(_adapterObjectPath) ? Connection.System.CreateProxy<IAdapter1>(BlueZConstants.BlueZDBusServiceName, _adapterObjectPath) : await FindFirstAdapter();
            
            // validate the adapter
            await adapter.GetAliasAsync();

            // make sure it is powered on
            if (!await adapter.GetPoweredAsync())
            {
                await adapter.SetPoweredAsync(true);
            }

            await adapter.WatchPropertiesAsync(AdapterPropertyChangedHandler);

            return adapter;
        }

        private async Task<IAdapter1> FindFirstAdapter()
        {
            var adapters = await Connection.System.FindProxies<IAdapter1>();
            return adapters.FirstOrDefault();
        }

        private void AdapterPropertyChangedHandler(PropertyChanges changes)
        {
            _logger.LogDebug("Property changed {ChangedProperties}", changes.Changed);

            foreach (var propertyChanged in changes.Changed)
            {
                switch (propertyChanged.Key)
                {
                    case "Discovering":
                        Discovering = (bool)propertyChanged.Value;
                        break;
                }
            }
        }

        private async Task<ICollection<IDevice1>> GetExistingDevicesAsync()
            => await Connection.System.FindProxies<IDevice1>();

        private IDevice1 GetSpecificDeviceAsync(ObjectPath objectPath)
            => Connection.System.CreateProxy<IDevice1>(BlueZConstants.BlueZDBusServiceName, objectPath);

        private async Task<bool> IsLegoWirelessProcotolDevice(IDevice1 device)
            => (await device.GetUUIDsAsync()).NullToEmpty().Any(x => x.ToUpperInvariant() == PoweredUpBluetoothConstants.LegoHubService);

        public async void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            _adapter ??= await GetAdapterAsync();

            var existingDevices = await GetExistingDevicesAsync();

            foreach (var device in existingDevices)
            {
                if (await IsLegoWirelessProcotolDevice(device))
                {
                    var poweredUpDevice = new BlueZPoweredUpBluetoothDevice(device, discoveryHandler);
                    await poweredUpDevice.Initialize();

                    _devices.Add(poweredUpDevice.DeviceInfo.BluetoothAddress, poweredUpDevice);

                    await poweredUpDevice.TryGetManufacturerDataAsync();
                }
            }

            await Connection.System.WatchInterfacesAdded(NewDeviceAddedHandler);

            await _adapter.SetDiscoveryFilterAsync(new Dictionary<string,object>()
            {
                { "UUIDs", new string[] { PoweredUpBluetoothConstants.LegoHubService } }
            });

            cancellationToken.Register(async () =>
            {
                if (Discovering)
                {
                    await _adapter.StopDiscoveryAsync();
                }
            });

            await _adapter.StartDiscoveryAsync();

            async void NewDeviceAddedHandler((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
            {
                if (!args.interfaces.ContainsKey("org.bluez.Device1"))
                {
                    return;
                }

                var device = GetSpecificDeviceAsync(args.objectPath);
                var poweredUpDevice = new BlueZPoweredUpBluetoothDevice(device, discoveryHandler);

                await poweredUpDevice.Initialize();
                
                _devices.Add(poweredUpDevice.DeviceInfo.BluetoothAddress, poweredUpDevice);

                await poweredUpDevice.TryGetManufacturerDataAsync();
            }
        }

        public Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            if (!_devices.ContainsKey(bluetoothAddress))
            {
                throw new ArgumentOutOfRangeException("Requested bluetooth device is not available from this adapter");
            }

            return Task.FromResult<IPoweredUpBluetoothDevice>(_devices[bluetoothAddress]);
        }
    }
}
