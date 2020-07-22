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

        public Hub CreateByBluetoothManufacturerData(byte[] manufacturerData)
            => (manufacturerData == null || manufacturerData.Length < 3) ? null : Create(GetSystemTypeFromManufacturerData((PoweredUpHubManufacturerData)manufacturerData[1]));

        private SystemType GetSystemTypeFromManufacturerData(PoweredUpHubManufacturerData poweredUpHubManufacturerData)
            => (SystemType)poweredUpHubManufacturerData;

        public Hub Create(SystemType hubType)
            => hubType switch
            {
                SystemType.LegoTechnic_MediumHub => ActivatorUtilities.CreateInstance<TechnicMediumHub>(_serviceProvider, (byte)0x00),
                _ => throw new NotSupportedException($"Hub with type {hubType} not yet supported."),
            };

        public THub Create<THub>() where THub : class
            => Create(GetSystemTypeFromType(typeof(THub))) as THub;

        public static SystemType GetSystemTypeFromType(Type type)
            => type.Name switch
            {
                nameof(TechnicMediumHub) => SystemType.LegoTechnic_MediumHub,
                _ => throw new NotSupportedException(),
            };
    }
}