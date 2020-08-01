using System;
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
        {
            var hub = (manufacturerData == null || manufacturerData.Length < 3) ? null : Create(GetTypeFromSystemType(GetSystemTypeFromManufacturerData((PoweredUpHubManufacturerData)manufacturerData[1])));
            hub.Configure(0x00);

            return hub;
        }

        public THub Create<THub>() where THub : Hub
        {
            var hub = Create(typeof(THub)) as THub;
            hub.Configure(0x00);

            return hub;
        }

        private Hub Create(Type type)
            => _serviceProvider.GetService(type) as Hub ?? throw new NotSupportedException($"Hub with type {type} not registered in service locator supported.");

        private SystemType GetSystemTypeFromManufacturerData(PoweredUpHubManufacturerData poweredUpHubManufacturerData)
            => (SystemType)poweredUpHubManufacturerData;

        public static Type GetTypeFromSystemType(SystemType systemType)
            => systemType switch
            {
                SystemType.LegoTechnic_MediumHub => typeof(TechnicMediumHub),
                _ => throw new NotSupportedException(),
            };

        public static SystemType GetSystemTypeFromType(Type type)
            => type.Name switch
            {
                nameof(TechnicMediumHub) => SystemType.LegoTechnic_MediumHub,
                _ => throw new NotSupportedException(),
            };
    }
}