using System;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Hubs
{
    public static class HubFactory
    {
        internal static Hub CreateByBluetoothManufacturerData(byte[] manufacturerData, IServiceProvider serviceProvider)
            => (manufacturerData == null || manufacturerData.Length < 3) ? null : (PoweredUpHubManufacturerData)manufacturerData[1] switch
            {
                PoweredUpHubManufacturerData.TechnicMediumHub => new TechnicMediumHub(0x00, serviceProvider),
                _ => throw new NotSupportedException($"Hub with type {(PoweredUpHubManufacturerData)manufacturerData[1]} not yet supported."),
            };

        public static SystemType GetSystemTypeFromType(Type type)
            => type.Name switch
            {
                nameof(TechnicMediumHub) => SystemType.LegoTechnic_MediumHub,
                _ => throw new NotSupportedException(),
            };
    }
}