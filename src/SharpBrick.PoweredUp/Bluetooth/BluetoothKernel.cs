using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public class BluetoothKernel : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPoweredUpBluetoothAdapter _bluetoothAdapter;
        public IPoweredUpBluetoothDeviceInfo BluetoothDeviceInfo { get; set; }
        private IPoweredUpBluetoothDevice _device = null;
        private IPoweredUpBluetoothService _service = null;
        private IPoweredUpBluetoothCharacteristic _characteristic = null;

        public BluetoothKernel(IPoweredUpBluetoothAdapter bluetoothAdapter, ILogger<BluetoothKernel> logger = default)
        {
            _bluetoothAdapter = bluetoothAdapter ?? throw new ArgumentNullException(nameof(bluetoothAdapter));
            _logger = logger;
        }

        public bool IsConnected => _characteristic is not null;

        public async Task ConnectAsync()
        {
            _device = await _bluetoothAdapter.GetDeviceAsync(BluetoothDeviceInfo);
            _service = await _device.GetServiceAsync(new Guid(PoweredUpBluetoothConstants.LegoHubService));
            _characteristic = await _service.GetCharacteristicAsync(new Guid(PoweredUpBluetoothConstants.LegoHubCharacteristic));

            _logger?.LogDebug("Connected");
        }


        public Task DisconnectAsync()
        {
            _characteristic = null;
            _service?.Dispose();
            _service = null;
            _device?.Dispose();
            _device = null;

            _logger?.LogDebug("Disconnected");

            return Task.CompletedTask;
        }

        public async Task SendBytesAsync(byte[] data)
        {
            if (_characteristic is null)
            {
                throw new InvalidOperationException($"Cannot invoke {nameof(SendBytesAsync)} when device is not connected ({nameof(_characteristic)} is null)");
            }

            _logger?.LogDebug($"> {BytesStringUtil.DataToString(data)}");

            await _characteristic.WriteValueAsync(data);

        }

        public async Task ReceiveBytesAsync(Func<byte[], Task> handler)
        {
            if (_characteristic is null)
            {
                throw new InvalidOperationException($"Cannot invoke {nameof(SendBytesAsync)} when device is not connected ({nameof(_characteristic)} is null)");
            }

            await _characteristic.NotifyValueChangeAsync(async data =>
            {
                _logger?.LogDebug($"< {BytesStringUtil.DataToString(data)}");

                await handler(data);
            });

            _logger?.LogDebug("Registered Receive Handler");
        }

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