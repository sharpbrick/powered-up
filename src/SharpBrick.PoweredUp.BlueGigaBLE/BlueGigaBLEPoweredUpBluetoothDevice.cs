using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BGLibExt;

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
        public Bluegiga.BGLib BgLib => BluetoothAdapter.BgLib;
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
        /// Is the device actually connected?
        /// </summary>
        public bool IsConnected { get; set; } = false;
        /// <summary>
        /// The Logger for this object
        /// </summary>
        public ILogger Logger { get; init; }
        /// <summary>
        /// The BleuGiga-Device-object of this device
        /// </summary>
        public BleDevice BleDevice { get; init; }
        /// <summary>
        /// The Services this device is currently aware of
        /// </summary>
        public ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService> GATTServices { get; }
        #endregion
        #region Constructors
        public BlueGigaBLEPoweredUpBluetoothDevice(ulong deviceAdress, byte[] deviceAdressBytes, string name, BlueGigaBLEPoweredUpBluetoothAdapater blueGigaBLEPoweredUpBluetoothAdapater, ILogger logger, bool traceDebug, BleDevice bleDevice)
        {
            Logger = logger;
            TraceDebug = traceDebug;
            DeviceAdress = deviceAdress;
            DeviceAdressBytes = deviceAdressBytes ?? throw new ArgumentNullException(nameof(deviceAdressBytes));
            BluetoothAdapter = blueGigaBLEPoweredUpBluetoothAdapater;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GATTServices = new ConcurrentDictionary<Guid, BlueGigaBLEPoweredUpBluetoothService>();
            BleDevice = bleDevice;
        }
        #endregion
        #region IPoweredUpBluetoothDevice
        public string Name { get; init; }
        public async Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        {
            var service = BleDevice.Services.FirstOrDefault(s => s.Uuid == serviceId);
            if (service != null)
            {
                var legoService = new BlueGigaBLEPoweredUpBluetoothService(this, serviceId, service.StartHandle, service.EndHandle, Logger, TraceDebug);
                return await Task.FromResult(GATTServices.AddOrUpdate(serviceId, legoService, (oldkey, oldvalue) => oldvalue = legoService));
            }
            else
            {
                throw new ArgumentException($"The device with address {DeviceAdress} ({DeviceAdressBytes}) DOES NOT KNOW a service with GUID {serviceId}!");
            }
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
                    if (BleDevice.IsConnected)
                    {
                        _ = BleDevice.DisconnectAsync();
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
                    $"{indentStr}Device-Adress (Byte[] little endian): { BlueGigaBLEHelper.ByteArrayToHexString(DeviceAdressBytes)}" + Environment.NewLine);
                if (GATTServices?.Count > 0)
                {
                    _ = sb.Append($"{indentStr}I know about the following {GATTServices.Count} services found during explicit use:{Environment.NewLine}");
                    foreach (var service in GATTServices)
                    {
                        _ = sb.Append(await service.Value.GetLogInfosAsync(indent + 1));
                    }

                    _ = sb.Append($"{indentStr}End of my known services which have been found during explicit use{Environment.NewLine}");
                }
                else
                {
                    _ = sb.Append($"{indentStr}I DON'T know about any services I should have!{Environment.NewLine}");
                }
                if (BleDevice.Services?.Count > 0)
                {
                    _ = sb.Append($"{indentStr}I know about the following {BleDevice.Services.Count} services found during connection to the device:{Environment.NewLine}");
                    var innerIndent = indentStr + "\t";
                    foreach (var service in BleDevice.Services)
                    {
                        _ = sb.Append($"{innerIndent}UUID: {service.Uuid}{Environment.NewLine}{innerIndent}Start-Handle: {service.StartHandle}{Environment.NewLine}{innerIndent}End-Handle: {service.EndHandle}{Environment.NewLine}");
                        if (service.Characteristics?.Count > 0)
                        {
                            _ = sb.Append($"{innerIndent}I know about the following characteristics:{Environment.NewLine}");
                            var innnerInnnerIndent = innerIndent + "\t";
                            foreach (var characteristic in service.Characteristics)
                            {
                                _ = sb.Append($"{innnerInnnerIndent}Characteristic - UUID: {characteristic.Uuid}{Environment.NewLine}{innnerInnnerIndent}Handle: {characteristic.Handle}{Environment.NewLine}{innnerInnnerIndent}Configuration-Handle: {characteristic.HandleCcc}{Environment.NewLine}{innnerInnnerIndent}Has configuration-handle: {characteristic.HasCcc}{Environment.NewLine}");
                            }
                        }
                    }
                    _ = sb.Append($"{indentStr}End of my discovered services{Environment.NewLine}");
                }
                else
                {
                    _ = sb.Append($"{indentStr}I DON'T know about any services found during connection!{Environment.NewLine}");
                }
                return sb;
            });
            return sb.ToString();
        }
        #endregion
    }
}
