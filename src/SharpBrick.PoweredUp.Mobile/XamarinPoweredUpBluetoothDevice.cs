using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Mobile
{
    public class XamarinPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private INativeDevice _deviceInfo;

        public XamarinPoweredUpBluetoothDevice(INativeDevice deviceInfo)
        {
            this._deviceInfo = deviceInfo;
        }

        public string Name { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            throw new NotImplementedException();
        }
    }
}
