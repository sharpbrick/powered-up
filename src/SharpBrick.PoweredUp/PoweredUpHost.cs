using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp
{
    public class PoweredUpHost
    {
        private readonly IPoweredUpBluetoothAdapter _bluetoothAdapter;
        public IServiceProvider ServiceProvider { get; }
        private readonly ILogger<PoweredUpHost> _logger;
        private ConcurrentBag<(PoweredUpBluetoothDeviceInfo Info, Hub Hub)> _hubs = new ConcurrentBag<(PoweredUpBluetoothDeviceInfo, Hub)>();

        public IEnumerable<Hub> Hubs => _hubs.Select(i => i.Hub);

        public PoweredUpHost(IPoweredUpBluetoothAdapter bluetoothAdapter, IServiceProvider serviceProvider, ILogger<PoweredUpHost> logger)
        {
            _bluetoothAdapter = bluetoothAdapter;
            ServiceProvider = serviceProvider;
            _logger = logger;
        }

        //ctr with auto-finding bt adapter

        public THub FindByType<THub>() where THub : Hub
            => Hubs.OfType<THub>().FirstOrDefault();

        public THub Find<THub>(ulong bluetoothAddress) where THub : Hub
            => _hubs.Where(h => h.Info.BluetoothAddress == bluetoothAddress).Select(i => i.Hub).FirstOrDefault() as THub;

        public THub FindById<THub>(byte hubId) where THub : Hub, IDisposable
            => _hubs.Where(h => h.Hub.HubId == hubId).Select(i => i.Hub).FirstOrDefault() as THub;
        public THub FindByName<THub>(string name) where THub : Hub, IDisposable
            => _hubs.Where(h => h.Info.Name == name).Select(i => i.Hub).FirstOrDefault() as THub;

        public void Discover(Func<Hub, Task> onDiscovery, CancellationToken token = default)
        {
            _bluetoothAdapter.Discover(deviceInfo =>
            {
                if (!_hubs.Any(i => i.Item1.BluetoothAddress == deviceInfo.BluetoothAddress))
                {
                    var hub = CreateProtocolScope(deviceInfo.BluetoothAddress, hubFactory => hubFactory.CreateByBluetoothManufacturerData(deviceInfo.ManufacturerData));

                    _hubs.Add((deviceInfo, hub));

                    _logger.LogInformation($"Discovered log of type {hub.GetType().Name} with name '{deviceInfo.Name}' on Bluetooth Address '{deviceInfo.BluetoothAddress}'");

                    onDiscovery(hub).Wait();
                }
            }, token);
        }

        public async Task<THub> DiscoverAsync<THub>(CancellationToken token = default)
        {
            var tcs = new TaskCompletionSource<THub>();

            Discover(hub =>
            {
                if (hub is THub tHub)
                {
                    tcs.SetResult(tHub);
                }

                return Task.CompletedTask;
            }, token);

            var hub = await tcs.Task;

            _logger.LogInformation($"End DiscoveryAsync for {typeof(THub).Name}");

            return hub;
        }

        public THub Create<THub>(ulong bluetoothAddress) where THub : Hub
        {
            var hub = CreateProtocolScope(bluetoothAddress, hubFactory => hubFactory.Create<THub>());

            _hubs.Add((new PoweredUpBluetoothDeviceInfo()
            {
                BluetoothAddress = bluetoothAddress,
                Name = string.Empty,
                ManufacturerData = Array.Empty<byte>(),
            }, hub));

            return hub;
        }

        private THub CreateProtocolScope<THub>(ulong bluetoothAddress, Func<IHubFactory, THub> factory) where THub : Hub
        {
            var scopedServiceProvider = ServiceProvider.CreateScope().ServiceProvider;

            var hubFactory = scopedServiceProvider.GetService<IHubFactory>();

            var hub = factory(hubFactory);
            hub.ConnectWithBluetoothAdapter(_bluetoothAdapter, bluetoothAddress);

            return hub;
        }
    }
}