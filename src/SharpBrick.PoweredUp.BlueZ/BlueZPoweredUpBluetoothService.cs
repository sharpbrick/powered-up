using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    public class BlueZPoweredUpBluetoothService : IPoweredUpBluetoothService
    {
        private readonly IGattService1 _service;
        private readonly ILogger _logger;

        public Guid Uuid => Guid.Parse(_service.GetUUIDAsync().Result);

        private readonly Dictionary<Guid, IGattCharacteristic1> _characteristicCache = new Dictionary<Guid, IGattCharacteristic1>();


        internal BlueZPoweredUpBluetoothService(IGattService1 service, ILogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid characteristicId)
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>("org.bluez", "/");
            var objects = await objectManager.GetManagedObjectsAsync();

            foreach (var obj in objects)
            {
                if (obj.Value.ContainsKey("org.bluez.GattCharacteristic1"))
                {
                    _logger.LogWarning("Service object found: {Key}", obj.Key);
                    var service = Connection.System.CreateProxy<IGattCharacteristic1>("org.bluez", obj.Key);

                    var uuid = await service.GetUUIDAsync();

                    _characteristicCache.Add(Guid.Parse(uuid), service);
                }
            }
            
            return new BlueZPoweredUpBluetoothCharacteristic(_characteristicCache[characteristicId], _logger);
        }
    }
}
