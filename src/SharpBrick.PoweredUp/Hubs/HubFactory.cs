using System;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.Hubs
{
    public class HubFactory : IHubFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public HubFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Hub CreateByBluetoothManufacturerData(byte[] manufacturerData, IServiceProvider serviceProvider)
            => (manufacturerData == null || manufacturerData.Length < 3) ? null : (PoweredUpHubManufacturerData)manufacturerData[1] switch
            {
                PoweredUpHubManufacturerData.TechnicMediumHub => ActivatorUtilities.CreateInstance<TechnicMediumHub>(_serviceProvider, (byte)0x00),
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