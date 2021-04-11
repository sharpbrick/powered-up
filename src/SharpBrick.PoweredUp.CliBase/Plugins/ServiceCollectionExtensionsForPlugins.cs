using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharpBrick.PoweredUp
{
    public static class ServiceCollectionExtensionsForPlugins
    {
        public const string BluetoothAdapterConfigKey = "BluetoothAdapter";
        public const string DefaultBluetoothAdapter = "WinRT";
        public const string BluetoothConfigConfigKey = "BluetoothConfig";

        public static IServiceCollection AddPoweredUpBluetooth(this IServiceCollection self, IConfiguration configuration)
        {
            var bleAdapter = configuration[BluetoothAdapterConfigKey] ?? DefaultBluetoothAdapter;

            var path = Path.Combine(Environment.CurrentDirectory, $"plugins", $"ble-{bleAdapter}", $"SharpBrick.PoweredUp.{bleAdapter}.dll");

            return AddPoweredUpPlugin(self, configuration, path);
        }

        public static IServiceCollection AddPoweredUpPlugin(IServiceCollection self, IConfiguration configuration, string path)
        {
            var loadContext = new PluginLoadContext(Path.GetFullPath(path));

            var assembly = loadContext.LoadFromAssemblyPath(path);

            foreach (var t in assembly.GetTypes())
            {
                if (t.GetInterfaces().Any(it => it == typeof(IPluginStartup)))
                {
                    var pluginStartup = Activator.CreateInstance(t) as IPluginStartup;

                    pluginStartup.Configure(self, configuration);
                }
            }

            return self;
        }
    }
}