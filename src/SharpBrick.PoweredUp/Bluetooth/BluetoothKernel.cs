using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public class BluetoothKernel : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPoweredUpBluetoothAdapter _bluetoothAdapter;
        private readonly ulong _bluetoothAddress;
        private IPoweredUpBluetoothDevice _device = null;
        private IPoweredUpBluetoothService _service = null;
        private IPoweredUpBluetoothCharacteristic _characteristic = null;

        public BluetoothKernel(IPoweredUpBluetoothAdapter bluetoothAdapter, ulong bluetoothAddress, ILogger<BluetoothKernel> logger = default)
        {
            _bluetoothAdapter = bluetoothAdapter ?? throw new ArgumentNullException(nameof(bluetoothAdapter));
            _bluetoothAddress = bluetoothAddress;
            _logger = logger;
        }

        public bool IsConnected => _characteristic != null;

        public async Task ConnectAsync()
        {
            _device = await _bluetoothAdapter.GetDeviceAsync(_bluetoothAddress);
            _service = await _device.GetServiceAsync(new Guid(PoweredUpBluetoothConstants.LegoHubService));
            _characteristic = await _service.GetCharacteristicAsync(new Guid(PoweredUpBluetoothConstants.LegoHubCharacteristic));

            _logger?.LogInformation("BluetoothKernel: Connected");
        }


        public async Task DisconnectAsync()
        {
            _characteristic = null;
            _service?.Dispose();
            _service = null;
            _device?.Dispose();
            _device = null;

            _logger?.LogInformation("BluetoothKernel: Disconnected");
        }

        public async Task SendBytesAsync(byte[] data)
        {
            if (_characteristic == null)
            {
                throw new InvalidOperationException($"Cannot invoke {nameof(SendBytesAsync)} when device is not connected ({nameof(_characteristic)} is null)");
            }

            _logger?.LogDebug($"BluetoothKernel: > {DataToString(data)}");

            await _characteristic.WriteValueAsync(data);

        }

        public async Task ReceiveBytesAsync(Func<byte[], Task> handler)
        {
            if (_characteristic == null)
            {
                throw new InvalidOperationException($"Cannot invoke {nameof(SendBytesAsync)} when device is not connected ({nameof(_characteristic)} is null)");
            }

            await _characteristic.NotifyValueChangeAsync(async data =>
            {
                _logger?.LogDebug($"BluetoothKernel: < {DataToString(data)}");

                await handler(data);
            });

            _logger?.LogInformation("BluetoothKernel: Registered Receive Handler");
        }

        public static string DataToString(byte[] data)
                => string.Join("-", data.Select(b => $"{b:X2}"));

        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // managed resource
                }

                // unmananged
                _service?.Dispose();
                _device?.Dispose();
                _device = null;

                disposedValue = true;
            }
        }

        ~BluetoothKernel() => Dispose(disposing: false);
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
        #endregion
    }
}