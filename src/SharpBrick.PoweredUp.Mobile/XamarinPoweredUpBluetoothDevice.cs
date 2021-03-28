using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private IDevice _device;
        private IAdapter _adapter;

        public string Name => this._device.Name;

        public XamarinPoweredUpBluetoothDevice(IDevice device, IAdapter bluetoothAdapter)
        {
            this._device = device;
            this._adapter = bluetoothAdapter;
        }

        #region IDisposible

        ~XamarinPoweredUpBluetoothDevice() => Dispose(false);

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

        protected virtual void Dispose(bool disposing)
        {
            _device?.Dispose();
            _device = null;
            _adapter = null;
        }

        #endregion

        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            await _adapter.ConnectToDeviceAsync(_device, new ConnectParameters(true, true)).ConfigureAwait(false);

            if (!_adapter.ConnectedDevices.Contains(_device)) return null;

            var service = await _device.GetServiceAsync(serviceId).ConfigureAwait(false);

            return new XamarinPoweredUpBluetoothService(service);
        }
    }
}
