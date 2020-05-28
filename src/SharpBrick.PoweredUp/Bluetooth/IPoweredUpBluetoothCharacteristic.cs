using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth
{
    public interface IPoweredUpBluetoothCharacteristic
    {
        Guid Uuid { get; }
        Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler);
        Task<bool> WriteValueAsync(byte[] data);
    }
}