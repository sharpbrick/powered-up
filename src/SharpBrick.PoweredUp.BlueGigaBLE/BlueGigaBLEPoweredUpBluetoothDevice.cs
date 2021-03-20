using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEPoweredUpBluetoothDevice : IPoweredUpBluetoothDevice, IBlueGigaLogger
    {
        #region Properties
        /// <summary>
        /// The instance of the blueGiga-library which is responible for the communication
        /// </summary>
        public Bluegiga.BGLib Bglib => BluetoothAdapter.Bglib;
        /// <summary>
        /// The adapter this device is attached to
        /// </summary>
        public BlueGigaBLEPoweredUpBluetoothAdapater BluetoothAdapter { get; init; }
        /// <summary>
        /// The device's adress in form of a ulong (computed from the adress-bytes/BLE-MAC-adress
        /// </summary>
        public ulong DeviceAdress { get; init; }
        /// <summary>
        /// The MAC-adress of the BLE-device in reversed order
        /// </summary>
        public byte[] DeviceAdressBytes { get; init; }
        /// <summary>
        /// The connection-handle this device has when it has been connected from the bluetooth-adapter;
        /// has to be set during the Connect() in the bluetooth-adapter
        /// </summary>
        public byte ConnectionHandle { get; init; }
        /// <summary>
        /// Is the device actually connected?
        /// </summary>
        public bool IsConnected { get; set; } = false;
        /// <summary>
        /// The Logger for this object
        /// </summary>
        private ILogger Logger { get; init; }
        /// <summary>
        /// The Services this device is currently aware of
        /// </summary>
        public ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService> GATTServices { get; }
        #endregion
        #region Constructors
        public BlueGigaBLEPoweredUpBluetoothDevice(ulong deviceAdress, byte[] deviceAdressBytes, string name, BlueGigaBLEPoweredUpBluetoothAdapater blueGigaBLEPoweredUpBluetoothAdapater, ILogger logger, bool traceDebug, byte connectionHandle)
        {
            Logger = logger;
            TraceDebug = traceDebug;
            DeviceAdress = deviceAdress;
            DeviceAdressBytes = deviceAdressBytes ?? throw new ArgumentNullException(nameof(deviceAdressBytes));
            BluetoothAdapter = blueGigaBLEPoweredUpBluetoothAdapater;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GATTServices = new ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService>();
            ConnectionHandle = connectionHandle;
        }
        #endregion
        #region IPoweredUpBluetoothDevice
        public string Name { get; init; }
        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            return await Task.FromResult(GATTServices[serviceId]);
        }
        #endregion
        #region IDisposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var service in GATTServices)
                    {
                        service.Value.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region IBlueGigaLogger
        public bool TraceDebug { get; init; }
        public async Task LogInfosAsync(int indent = 0, string header = "", string footer = "")
        {
            if (TraceDebug)
            {
                await Task.Run(async () =>
                {
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(header))
                    {
                        _ = sb.Append(header.ToUpper());
                    }
                    _ = sb.Append(await GetLogInfosAsync(indent));
                    if (!string.IsNullOrEmpty(footer))
                    {
                        _ = sb.Append(footer.ToUpper());
                    }
                    Logger?.LogDebug(sb.ToString());
                });
            }
        }

        public async Task<string> GetLogInfosAsync(int indent)
        {
            var sb = new StringBuilder();
            _ = await Task.Run(async () =>
            {
                var indentStr = new string('\t', indent < 0 ? 0 : indent);
                _ = sb.Append(
                $"{indentStr}*** Device-Info ***:" + Environment.NewLine +
                    $"{indentStr}Device-Adress (ulong): {DeviceAdress}" + Environment.NewLine +
                    $"{indentStr}Device-Adress (Byte[] little endian): { BlueGigaBLEHelper.ByteArrayToHexString(DeviceAdressBytes)}" + Environment.NewLine +
                    (IsConnected ?
                        $"{indentStr}Connection-Handle on my Bluetooth-Adapter: { ConnectionHandle }" :
                    $"{indentStr}Actually I'm not connected to any Bluetooth-Adapter") + Environment.NewLine);
                if (GATTServices?.Count > 0)
                {
                    _ = sb.Append($"{indentStr}I know about the following {GATTServices.Count} services:");
                    foreach (var service in GATTServices)
                    {
                        _ = sb.Append(await service.Value.GetLogInfosAsync(indent + 1));
                    }

                    _ = sb.Append($"{indentStr}End of my known services");
                }
                else
                {
                    _ = sb.Append($"{indentStr}I DON'T know about any services I should have!");
                }
                return sb;
            });
            return sb.ToString();
        }
        #endregion

    }
}
