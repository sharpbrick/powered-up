using System;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Hubs
{
    public static class HubFactory
    {
        internal static Hub CreateByBluetoothManufacturerData(byte[] manufacturerData, IServiceProvider serviceProvider)
            => (manufacturerData == null || manufacturerData.Length < 3) ? null : (PoweredUpHubManufacturerData)manufacturerData[1] switch
            {
                PoweredUpHubManufacturerData.TechnicMediumHub => new TechnicMediumHub(serviceProvider),
            };
    }
}