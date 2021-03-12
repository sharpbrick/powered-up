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
        public BlueGigaBLEPoweredUpBluetoothDevice myDevice { get; }
        /// <summary>
        /// The connection (of the device) over which the service is reachable (redundant, but useful for information/log)
        /// </summary>
        public Byte connection { get { return myDevice.ConnectionHandle; } }
        /// <summary>
        /// First handle of a character this service has
        /// </summary>
        public ushort firstCharacterHandle { get; }
        /// <summary>
        /// Last handle of a character this service has
        /// </summary>
        public ushort lastCharacterHandle { get; }
        
        /// <summary>
        /// The Logger for this object
        /// </summary>
        public readonly ILogger _logger;
        /// <summary>
        /// Dictionary of Characteristics this service has to offer; will be filled on Discover or DirectConnect of the device!
        /// </summary>
        public ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothCharacteristic> myCharacteristics;
        #endregion
        #region Constructors
        /// <summary>
        /// Construcor for the BLE-Service
        /// </summary>
        /// <param name="myDevice">The device which offers this service</param>
        /// <param name="serviceUuid">The service-UUID of this service</param>
        /// <param name="firstCharacterHandle">The first handle under which the service has a characteristic</param>
        /// <param name="lastCharacterHandle">The last handle under which the service has a characteristic</param>
        public BlueGigaBLEPoweredUpBluetoothService(BlueGigaBLEPoweredUpBluetoothDevice myDevice, Guid serviceUuid, ushort firstCharacterHandle, ushort lastCharacterHandle, ILogger logger, bool traceDebug)
        {
            this.myDevice = myDevice ?? throw new ArgumentNullException(nameof(myDevice));
            this.Uuid = serviceUuid;
            this.firstCharacterHandle = firstCharacterHandle;
            this.lastCharacterHandle = lastCharacterHandle;
            _logger = logger;
            TraceDebug = traceDebug;
            this.myCharacteristics = new ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothCharacteristic>();
        }
        #endregion
        #region IPoweredUpBluetoothService
        /// <summary>
        /// The service-UUID by which this service is uniquly identified on the device
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Get a Cahracteristic by its Characteristic-UUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
        {
            return await Task.FromResult(myCharacteristics[guid]);
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
                    foreach (KeyValuePair<Guid , BlueGigaBLEPoweredUpBluetoothCharacteristic> characteristic in this.myCharacteristics)
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
        public bool TraceDebug { get; set; }
        public void LogMyInfos(int indent = 0, String header = "", String footer = "")
        {
            if (TraceDebug)
            {
                StringBuilder sb = new StringBuilder();
                if (!String.IsNullOrEmpty(header)) sb.Append(header.ToUpper());
                sb.Append(GetMyLogInfos(indent));
                if (!String.IsNullOrEmpty(footer)) sb.Append(footer.ToUpper());
                _logger?.LogDebug(sb.ToString());
            }
        }
        public string GetMyLogInfos(int indent)
        {
            String indentStr = new String('\t', indent < 0 ? 0 : indent);
            StringBuilder sb = new StringBuilder();
            sb.Append($"{indentStr}*** Service-Info ***:" + Environment.NewLine +
                    $"{indentStr}Service-UUID: {this.Uuid}" + Environment.NewLine +
                    $"{indentStr}First Characteristic-Handle: { this.firstCharacterHandle}" + Environment.NewLine +
                    $"{indentStr}Last Characteristic-Handle: { this.lastCharacterHandle}" + Environment.NewLine +
                    $"{indentStr}I'm connected on handle {myDevice.ConnectionHandle} on Device [{BlueGigaBLEHelper.ByteArrayToHexString(myDevice.DeviceAdressBytes)}] [{myDevice.DeviceAdress}]" + Environment.NewLine);
            if (this.myCharacteristics?.Count > 0)
            {
                sb.Append($"{indentStr}I know about the following {this.myCharacteristics.Count} characteristics:");
                foreach (KeyValuePair<Guid, BlueGigaBLEPoweredUpBluetoothCharacteristic> characteristic in this.myCharacteristics)
                    sb.Append(characteristic.Value.GetMyLogInfos(indent + 1));
                sb.Append($"{indentStr}End of my known characteristics");
            }
            else
                sb.Append($"{indentStr}I DON'T know about any characteristics I should have!");
            return sb.ToString();

        }
        #endregion
    }
}
