using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEPoweredUpBluetoothService : IPoweredUpBluetoothService, IBlueGigaLogger
    {
        #region Properties
        /// <summary>
        /// The device (LEGO-Hub) on which this service is available
        /// </summary>
        public BlueGigaBLEPoweredUpBluetoothDevice Device { get; }
        /// <summary>
        /// The connection (of the device) over which the service is reachable (redundant, but useful for information/log)
        /// </summary>
        public byte Connection => Device.ConnectionHandle;
        /// <summary>
        /// First handle of a character this service has
        /// </summary>
        public ushort FirstCharacterHandle { get; init; }
        /// <summary>
        /// Last handle of a character this service has
        /// </summary>
        public ushort LastCharacterHandle { get; init; }
        /// <summary>
        /// The Logger for this object
        /// </summary>
        private ILogger Logger { get; init; }
        /// <summary>
        /// Dictionary of Characteristics this service has to offer; will be filled on Discover or DirectConnect of the device!
        /// </summary>
        public ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothCharacteristic> GATTCharacteristics { get; }
        #endregion
        #region Constructors
        /// <summary>
        /// Construcor for the BLE-Service
        /// </summary>
        /// <param name="device">The device which offers this service</param>
        /// <param name="serviceUuid">The service-UUID of this service</param>
        /// <param name="firstCharacterHandle">The first handle under which the service has a characteristic</param>
        /// <param name="lastCharacterHandle">The last handle under which the service has a characteristic</param>
        public BlueGigaBLEPoweredUpBluetoothService(BlueGigaBLEPoweredUpBluetoothDevice device, Guid serviceUuid, ushort firstCharacterHandle, ushort lastCharacterHandle, ILogger logger, bool traceDebug)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            Uuid = serviceUuid;
            FirstCharacterHandle = firstCharacterHandle;
            LastCharacterHandle = lastCharacterHandle;
            Logger = logger;
            TraceDebug = traceDebug;
            GATTCharacteristics = new ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothCharacteristic>();
        }
        #endregion
        #region IPoweredUpBluetoothService
        /// <summary>
        /// The service-UUID by which this service is uniquly identified on the device
        /// </summary>
        public Guid Uuid { get; init; }
        /// <summary>
        /// Get a Cahracteristic by its Characteristic-UUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            return await Task.FromResult(GATTCharacteristics[guid]);
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
                    foreach (var characteristic in GATTCharacteristics)
                    {
                        characteristic.Value.Dispose();
                    }
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
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
            var indentStr = new string('\t', indent < 0 ? 0 : indent);
            var sb = new StringBuilder();
            _ = await Task.Run(async () =>
            {
                _ = sb.Append($"{indentStr}*** Service-Info ***:" + Environment.NewLine +
                        $"{indentStr}Service-UUID: {Uuid}" + Environment.NewLine +
                        $"{indentStr}First Characteristic-Handle: { FirstCharacterHandle}" + Environment.NewLine +
                        $"{indentStr}Last Characteristic-Handle: { LastCharacterHandle}" + Environment.NewLine +
                        $"{indentStr}I'm connected on handle {Device.ConnectionHandle} on Device [{BlueGigaBLEHelper.ByteArrayToHexString(Device.DeviceAdressBytes)}] [{Device.DeviceAdress}]" + Environment.NewLine);
                if (GATTCharacteristics?.Count > 0)
                {
                    _ = sb.Append($"{indentStr}I know about the following {GATTCharacteristics.Count} characteristics:");
                    foreach (var characteristic in GATTCharacteristics)
            {
                        _ = sb.Append(await characteristic.Value.GetLogInfosAsync(indent + 1));
                    }

                    _ = sb.Append($"{indentStr}End of my known characteristics");
            }
            else
                {
                    _ = sb.Append($"{indentStr}I DON'T know about any characteristics I should have!");
                }
                return sb;
            });
            return sb.ToString();

        }
        #endregion
    }
}
