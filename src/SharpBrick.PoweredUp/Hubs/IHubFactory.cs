using System;

namespace SharpBrick.PoweredUp.Hubs
{
    public interface IHubFactory
    {
        Hub CreateByBluetoothManufacturerData(byte[] manufacturerData, IServiceProvider serviceProvider);
    }
}