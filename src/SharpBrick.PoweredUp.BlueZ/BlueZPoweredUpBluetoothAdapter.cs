using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.BlueZ.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothAdapter : IPoweredUpBluetoothAdapter
    {
        private readonly IAdapter1 _adapter;
        private readonly BluetoothAddressFormatter _bluetoothAddressFormatter;
        private readonly ILogger<BlueZPoweredUpBluetoothAdapter> _logger;

        internal bool IsDiscovering { get; private set; }= false;

        private readonly Dictionary<ulong, IDevice1> _deviceCache = new Dictionary<ulong, IDevice1>();

        public BlueZPoweredUpBluetoothAdapter(BluetoothAddressFormatter bluetoothAddressFormatter, ILogger<BlueZPoweredUpBluetoothAdapter> logger)
        {
            _logger = logger;
            _bluetoothAddressFormatter = bluetoothAddressFormatter;
            _adapter = GetAdapterAsync().Result;
            
            AttachEventHandlers().Wait();
            _adapter.SetDiscoveryFilterAsync(new Dictionary<string,object>()
            {
                { "UUIDs", new string[] { PoweredUpBluetoothConstants.LegoHubService } }
            });
        }

        internal async Task StopDiscoveryAsync()
        {
            await _adapter.StopDiscoveryAsync();
        }

        private async Task<IAdapter1> GetAdapterAsync()
        {
            //var objectManager = Connection.System.CreateProxy<IObjectManager>("org.bluez", "/");

            //var objects = await objectManager.GetManagedObjectsAsync();

            var adapter = Connection.System.CreateProxy<IAdapter1>("org.bluez","/org/bluez/hci0");
            
            // check if the adapter is valid
            await adapter.GetAliasAsync();

            return adapter;
        }

        private async Task AttachEventHandlers()
        {
            await _adapter.WatchPropertiesAsync((propertyChanges) => {
                _logger.LogWarning("Property changed {ChangedProperties} - {Invalidated}", propertyChanges.Changed, propertyChanges.Invalidated);
                foreach (var change in propertyChanges.Changed)
                {
                    if (change.Key == "Discovering")
                    {
                        IsDiscovering = (bool)change.Value;
                    }
                }
            });
        }

        public async void Discover(Action<PoweredUpBluetoothDeviceInfo> discoveryHandler, CancellationToken cancellationToken = default)
        {

            cancellationToken.Register(async () =>
            {
                //await _adapter.StopDiscoveryAsync();
            });

            //await _adapter.StartDiscoveryAsync();

            // make sure handler is executed for existing devices, since they will not be found by the interface watcher
            await GetExistingDevicesAsync(DeviceFoundHandler);

            var objectManager = Connection.System.CreateProxy<IObjectManager>("org.bluez", "/");
            var disposable = await objectManager.WatchInterfacesAddedAsync(DeviceFoundHandler);


            async void DeviceFoundHandler((ObjectPath path, IDictionary<string, IDictionary<string, object>> interfaces) args)
            {
                if(args.interfaces.ContainsKey("org.bluez.Device1"))
                {
                    _logger.LogWarning("Device found: {Path}, {Interfaces}", args.path, args.interfaces);

                    var info = new PoweredUpBluetoothDeviceInfo();

                    var device = Connection.System.CreateProxy<IDevice1>("org.bluez", args.path);

                    var manufacturerData = await device.GetManufacturerDataAsync();

                    if (manufacturerData.Count > 0)
                    {
                        info.ManufacturerData = (byte[])manufacturerData.First().Value;

                        info.Name = await device.GetNameAsync();
                    }

                    var btAddress = await device.GetAddressAsync();

                    info.BluetoothAddress = _bluetoothAddressFormatter.ConvertToInteger(btAddress);

                    _deviceCache.Add(info.BluetoothAddress, device);

                    discoveryHandler(info);
                }
            }

        }

        public async Task GetExistingDevicesAsync(Action<(ObjectPath path, IDictionary<string, IDictionary<string, object>> interfaces)> handler)
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>("org.bluez", "/");
            var objects = await objectManager.GetManagedObjectsAsync();

            foreach (var obj in objects)
            {
                if (obj.Value.ContainsKey("org.bluez.Device1"))
                {
                    _logger.LogWarning("Device object found: {Key}", obj.Key);

                    handler((obj.Key, obj.Value));
                }
            }
        }

        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            if (!_deviceCache.ContainsKey(bluetoothAddress))
            {
                throw new ArgumentOutOfRangeException("Requested bluetooth device is not properly discovered yet");
            }

            //await _adapter.StopDiscoveryAsync();
            
            return new BlueZPoweredUpBluetoothDevice(_deviceCache[bluetoothAddress], this, _logger);
        }
    }
}
