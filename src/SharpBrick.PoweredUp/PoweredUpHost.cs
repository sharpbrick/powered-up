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
using SharpBrick.PoweredUp.Protocol;

namespace SharpBrick.PoweredUp
{
    public class PoweredUpHost
    {
        private readonly IPoweredUpBluetoothAdapter _bluetoothAdapter;
        public IServiceProvider ServiceProvider { get; }
        private readonly ILogger<PoweredUpHost> _logger;
        private readonly ConcurrentBag<(PoweredUpBluetoothDeviceInfo Info, Hub Hub)> _hubs = new();

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
            _bluetoothAdapter.Discover(async deviceInfo =>
            {
                try
                {
                    if (!_hubs.Any(i => i.Info.BluetoothAddress == deviceInfo.BluetoothAddress))
                    {
                        var hub = CreateHubInBluetoothScope(deviceInfo.BluetoothAddress, hubFactory => hubFactory.CreateByBluetoothManufacturerData(deviceInfo.ManufacturerData));

                        _hubs.Add((deviceInfo, hub));

                        _logger.LogInformation($"Discovered log of type {hub.GetType().Name} with name '{deviceInfo.Name}' on Bluetooth Address '{deviceInfo.BluetoothAddress}'");

                        await onDiscovery(hub);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception discovery handler within {nameof(Discover)}");

                    throw;
                }
            }, token);
        }

        public async Task<THub> DiscoverAsync<THub>(CancellationToken token = default) where THub : class
                    => await DiscoverInternalAsync(typeof(THub), token) as THub;

        public async Task<Hub> DiscoverAsync(Type hubType, CancellationToken token = default)
            => await DiscoverInternalAsync(hubType, token);

        private async Task<Hub> DiscoverInternalAsync(Type hubType, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<Hub>();

            Discover(hub =>
            {
                var currentHubType = hub.GetType();

                if (currentHubType == hubType)
                {
                    tcs.SetResult(hub);
                }

                return Task.CompletedTask;
            }, token);

            var hub = await tcs.Task;

            _logger.LogInformation($"End DiscoveryAsync for {hubType.Name}");
            return hub;
        }

        public THub Create<THub>(ulong bluetoothAddress) where THub : Hub
        {
            var hub = CreateHubInBluetoothScope(bluetoothAddress, hubFactory => hubFactory.Create<THub>());

            _hubs.Add((new PoweredUpBluetoothDeviceInfo()
            {
                BluetoothAddress = bluetoothAddress,
                Name = string.Empty,
                ManufacturerData = Array.Empty<byte>(),
            }, hub));

            return hub;
        }

        public ILegoWirelessProtocol CreateProtocol(ulong bluetoothAddress)
        {
            var protocolScope = CreateProtocolScope(bluetoothAddress);

            var protocol = protocolScope.ServiceProvider.GetService<ILegoWirelessProtocol>();

            return protocol;
        }

        public IServiceScope CreateProtocolScope(ulong bluetoothAddress)
        {
            var scope = ServiceProvider.CreateScope();
            var scopedServiceProvider = scope.ServiceProvider;

            // initialize scoped bluetooth kernel to bluetooth address.
            var kernel = scopedServiceProvider.GetService<BluetoothKernel>();
            kernel.BluetoothAddress = bluetoothAddress;

            return scope;
        }

        private THub CreateHubInBluetoothScope<THub>(ulong bluetoothAddress, Func<IHubFactory, THub> factory) where THub : Hub
        {
            var protocolScope = CreateProtocolScope(bluetoothAddress);

            var hubFactory = protocolScope.ServiceProvider.GetService<IHubFactory>();

            var hub = factory(hubFactory);

            return hub;
        }
    }
}