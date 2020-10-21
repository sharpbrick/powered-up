using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SharpBrick.PoweredUp.BlueZ
{
    internal static class DBusConnectionExtensions
    {
        internal static async Task<ICollection<T>> FindProxies<T>(this Connection connection) where T : IDBusObject
        {
            var dbusInterfaceAttribute = typeof(T).GetCustomAttributes(false).Cast<DBusInterfaceAttribute>().First();
            var objects = await GetObjectManager(connection).GetManagedObjectsAsync();

            return objects
                .Where(x => x.Value.ContainsKey(dbusInterfaceAttribute.Name))
                .Select(x => Connection.System.CreateProxy<T>(BlueZConstants.BlueZDBusServiceName, x.Key))
                .ToList();
        }

        internal static async Task WatchInterfacesAdded(this Connection connection, Action<(ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces)> handler)
        {
            var disposable = await GetObjectManager(connection).WatchInterfacesAddedAsync(handler);
        }

        private static IObjectManager GetObjectManager(Connection connection)
            => connection.CreateProxy<IObjectManager>(BlueZConstants.BlueZDBusServiceName, "/");
    }
}
