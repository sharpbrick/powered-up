using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private readonly IDevice1 _device;
        private readonly BlueZPoweredUpBluetoothAdapter _adapter;
        private readonly ILogger _logger;

        public bool IsConnected { get; private set; } = false;
        public bool ServicesResolved { get; private set; } = false; 

        private readonly Dictionary<Guid, IGattService1> _serviceCache = new Dictionary<Guid, IGattService1>();


        public string Name => _device.GetNameAsync().Result;

        internal BlueZPoweredUpBluetoothDevice(IDevice1 device, BlueZPoweredUpBluetoothAdapter adapter, ILogger logger)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _adapter = adapter;
            _logger = logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // private  async Task AttachEventHandlers()
        // {
        //     // if (_adapter.IsDiscovering)
        //     // {
        //     //     await _adapter.StopDiscoveryAsync();
        //     // }

        //     // await Task.Delay(TimeSpan.FromSeconds(2));

        //     // if (_adapter.IsDiscovering)
        //     // {
        //     //     throw new Exception("Cannot watch device properties if the adapter is discovering");
        //     // }

        // }

        private void PropertyChangedHandler(PropertyChanges changes)
        {
            foreach (var changed in changes.Changed)
            {
                if (changed.Key == "Connected")
                {
                    IsConnected = (bool)changed.Value;
                }

                if (changed.Key == "ServicesResolved")
                {
                    ServicesResolved = (bool)changed.Value;
                }
            }
        }

        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(15);

            await _device.WatchPropertiesAsync(PropertyChangedHandler).ConfigureAwait(false);

            try
            {
                await _device.ConnectAsync().ConfigureAwait(false);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "error connecting to device");
                return null;
            }

            // if (_adapter.IsDiscovering)
            // {
            //     await _adapter.StopDiscoveryAsync();
            // }

            // await Task.Delay(TimeSpan.FromSeconds(2));

            // if (_adapter.IsDiscovering)
            // {
            //     throw new Exception("Cannot watch device properties if the adapter is discovering");
            // }

            // very crude, wait 5 seconds, convert property changed event handling to reactive code

            var isConnected = await _device.GetAsync<bool>("Connected");
            _logger.LogWarning("IsConnected = {IsConnected}", isConnected);
            var servicesResolved = await _device.GetAsync<bool>("ServicesResolved");
            _logger.LogWarning("ServicesResolved = {ServicesResolved}", servicesResolved);

            if (!isConnected || !servicesResolved)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (!IsConnected || !ServicesResolved)
                {
                    throw new Exception("Connection failed after 10 seconds");
                }
            }

            var objectManager = Connection.System.CreateProxy<IObjectManager>("org.bluez", "/");
            var objects = await objectManager.GetManagedObjectsAsync();

            foreach (var obj in objects)
            {
                if (obj.Value.ContainsKey("org.bluez.GattService1"))
                {
                    _logger.LogWarning("Service object found: {Key}", obj.Key);
                    var service = Connection.System.CreateProxy<IGattService1>("org.bluez", obj.Key);

                    var uuid = await service.GetUUIDAsync();

                    _serviceCache.Add(Guid.Parse(uuid), service);
                }
            }

            return new BlueZPoweredUpBluetoothService(_serviceCache[serviceId], _logger);
        }
    }
}
