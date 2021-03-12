using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluegiga.BLE.Events.ATTClient;
using Bluegiga.BLE.Events.Connection;
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
        public Bluegiga.BGLib Bglib { get { return this.myBluetoothAdapter.Bglib; } }
        /// <summary>
        /// The adapter this device is attached to
        /// </summary>
        public BlueGigaBLEPoweredUpBluetoothAdapater myBluetoothAdapter { get; set; }
        /// <summary>
        /// The device's adress in form of a ulong (computed from the adress-bytes/BLE-MAC-adress
        /// </summary>
        public ulong DeviceAdress { get; set; }
        /// <summary>
        /// The MAC-adress of the BLE-device in reversed order
        /// </summary>
        public Byte[] DeviceAdressBytes { get; set; }
        /// <summary>
        /// The connection-handle this device has when it has been connected from the bluetooth-adapter;
        /// has to be set during the Connect() in the bluetooth-adapter
        /// </summary>
        public Byte ConnectionHandle { get; set; }
        /// <summary>
        /// Is the device actually connected?
        /// </summary>
        public bool IsConnected = false;
        /// <summary>
        /// The Logger for this object
        /// </summary>
        public readonly ILogger _logger;
        /// <summary>
        /// The Services this device is currently aware of
        /// </summary>
        public ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService> myServices;
        #endregion
        #region Constructors
        public BlueGigaBLEPoweredUpBluetoothDevice(ulong deviceAdress, byte[] deviceAdressBytes, String name, BlueGigaBLEPoweredUpBluetoothAdapater blueGigaBLEPoweredUpBluetoothAdapater, ILogger logger, bool traceDebug)
        {
            _logger = logger;
            TraceDebug = traceDebug;
            DeviceAdress = deviceAdress;
            DeviceAdressBytes = deviceAdressBytes ?? throw new ArgumentNullException(nameof(deviceAdressBytes));
            myBluetoothAdapter = blueGigaBLEPoweredUpBluetoothAdapater;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            myServices = new ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService>();
        }
        #endregion
        #region IPoweredUpBluetoothDevice
        public string Name { get; set; }
        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            return await Task.FromResult(myServices[serviceId]);
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
                    foreach (KeyValuePair<Guid, BlueGigaBLEPoweredUpBluetoothService> service in this.myServices)
                    {
                        service.Value.Dispose();
                    }
                }
                disposedValue = true;
            }
        }
        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BlueGigaBLEPoweredUpBluetoothDevice()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region IBlueGigaLogger
        public bool TraceDebug { get; set; }
        public void LogMyInfos(int indent = 0, String header = "", String footer="")
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
            StringBuilder sb = new StringBuilder();
            String indentStr = new String('\t', indent < 0 ? 0 : indent);
            sb.Append(
                $"{indentStr}*** Device-Info ***:" + Environment.NewLine +
                $"{indentStr}Device-Adress (ulong): {this.DeviceAdress}" + Environment.NewLine +
                $"{indentStr}Device-Adress (Byte[] little endian): { BlueGigaBLEHelper.ByteArrayToHexString(this.DeviceAdressBytes)}" + Environment.NewLine +
                (this.IsConnected ?
                    $"{indentStr}Connection-Handle on my Bluetooth-Adapter: { this.ConnectionHandle }" :
                    $"{indentStr}Actually I'm not connected to any Bluetooth-Adapter") + Environment.NewLine);
            if (this.myServices?.Count > 0)
            {
                sb.Append($"{indentStr}I know about the following {this.myServices.Count} services:");
                foreach (KeyValuePair<Guid, BlueGigaBLEPoweredUpBluetoothService> service in this.myServices)
                    sb.Append(service.Value.GetMyLogInfos(indent + 1));
                sb.Append($"{indentStr}End of my known services");
            }
            else
                sb.Append($"{indentStr}I DON'T know about any services I should have!");
            return sb.ToString();
        }
        #endregion

    }
}
