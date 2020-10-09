using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice
    {
        private readonly HashtagChris.DotNetBlueZ.Device _device;

        public string Name => _device.GetNameAsync().Result;

        public BlueZPoweredUpBluetoothDevice(HashtagChris.DotNetBlueZ.Device gattDevice)
        {
            _device = gattDevice ?? throw new ArgumentNullException(nameof(gattDevice));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(15);
            try
            {
                await _device.ConnectAsync();
                await _device.WaitForPropertyValueAsync("Connected", value: true, timeout);
                await _device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);
            } 
            catch (TimeoutException)
            {
                return null;
            }

            var service = await _device.GetServiceAsync(serviceId.ToString());

            return new BlueZPoweredUpBluetoothService(service);
        }
    }
}
