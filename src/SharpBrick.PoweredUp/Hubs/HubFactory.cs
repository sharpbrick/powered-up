using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Hubs
{
    public static class HubFactory
    {
        internal static Hub CreateByBluetoothManufacturerData(byte[] manufacturerData, Microsoft.Extensions.Logging.ILogger logger)
            => (manufacturerData == null || manufacturerData.Length < 3) ? null : (PoweredUpHubManufacturerData)manufacturerData[1] switch
            {
                PoweredUpHubManufacturerData.TechnicMediumHub => new TechnicMediumHub(logger),
            };
    }
}