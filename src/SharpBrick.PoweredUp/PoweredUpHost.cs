using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp
{
    public class PoweredUpHost
    {
        private readonly IPoweredUpBluetoothAdapter _bluetoothAdapter;
        private readonly ILogger<PoweredUpHost> _logger;

        private ConcurrentDictionary<ulong, Hub> _hubs = new ConcurrentDictionary<ulong, Hub>();

        public IEnumerable<Hub> Hubs => _hubs.Values;

        public PoweredUpHost(IPoweredUpBluetoothAdapter bluetoothAdapter, ILogger<PoweredUpHost> logger = default)
        {
            _bluetoothAdapter = bluetoothAdapter;
            _logger = logger;
        }

        //ctr with auto-finding bt adapter

        public THub FindByType<THub>() where THub : Hub
            => _hubs.Values.OfType<THub>().FirstOrDefault();

        public THub Find<THub>(ulong bluetoothAddress) where THub : Hub
            => _hubs.TryGetValue(bluetoothAddress, out var hub) ? hub as THub : default;

        public THub FindById<THub>(byte hubId) where THub : Hub, IDisposable
            => _hubs.Values.FirstOrDefault(h => h.HubId == hubId) as THub;

        public void Discover(Func<Hub, Task> onDiscovery, CancellationToken token = default)
        {
            _bluetoothAdapter.Discover(deviceInfo =>
            {
                if (!_hubs.ContainsKey(deviceInfo.BluetoothAddress))
                {
                    var hub = HubFactory.CreateByBluetoothManufacturerData(deviceInfo.ManufacturerData, _logger);
                    hub.ConnectWithBluetoothAdapter(_bluetoothAdapter, deviceInfo.BluetoothAddress);

                    _hubs.TryAdd(deviceInfo.BluetoothAddress, hub);

                    onDiscovery(hub).Wait();
                }
            }, token);
        }
    }
}