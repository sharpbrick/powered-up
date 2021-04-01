using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using SharpBrick.PoweredUp.Bluetooth;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using BGLibExt;
using System.Collections.Generic;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEPoweredUpBluetoothAdapater : IPoweredUpBluetoothAdapter, IDisposable, IBlueGigaLogger
    {
        #region Properties
        /// <summary>
        /// The BGLib-obejct which does the low-level communication over the BlueGiga-adapter
        /// </summary>
        public Bluegiga.BGLib BgLib { get; init; }
        /// <summary>
        /// The high-level Connection-object to the Bluegiga-device
        /// </summary>
        public BleModuleConnection BleModuleConnection { get; init; }
        /// <summary>
        /// The serial connection over which the BleuGiga-adapter communicates
        /// </summary>
        public SerialPort SerialAPI => BleModuleConnection.SerialPort;
        /// <summary>
        /// The ILogger-instance
        /// </summary>
        private ILogger Logger { get; init; }
        /// <summary>
        /// The devices this adapter knows about (by Discovery or direct Connect)
        /// </summary>
        private ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice> Devices { get; }
        private ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo> DevicesInfo { get; }
        #endregion

        #region Constructor
        public BlueGigaBLEPoweredUpBluetoothAdapater(string comPortName, bool traceDebug, ILogger logger = default)
        {
            Logger = logger;
            TraceDebug = traceDebug;
            BgLib = new Bluegiga.BGLibDebug();
            BleModuleConnection = new BleModuleConnection(BgLib);
            BleModuleConnection.Start(comPortName, 0);
            Devices = new ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice>();
            DevicesInfo = new ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo>();
        }
        #endregion

        #region Interface IPoweredUpBluetoothAdapter
        /// <summary>
        /// Start scanning for BLE-advertis-packages and, when found FIRST COMPLETE advertisment from a lego-device, await the discoveryHandler.
        /// The found device DOES NOT get connected here!
        /// This may fire the discoveryHandler multiple times for THE SAME Hub; so it is the responsibility of the caller to filter double Hubs until the caller
        /// cancels the cancellationToken, which is the ONLY WAY to end the discovery-mode of the adapter
        /// </summary>
        /// <param name="discoveryHandler">The handler form the SharpBrick.PoweredUp-core which shall be called when a device has been discovered</param>
        /// <param name="cancellationToken">Cancelation-token to handle a cancel of the discovery</param>
        public void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            var bleDeviceDiscovery = new BleDeviceDiscovery(BgLib, BleModuleConnection);
            var bleParsedData = new ConcurrentDictionary<ulong, List<BleAdvertisingData>>();
            var myGAPScanResponseHandler = new BleDeviceDiscovery.ScanResponseReceivedEventHandler(GAPScanResponseEvent);
            async void GAPScanResponseEvent(object sender, BleScanResponseReceivedEventArgs e)
            {
                //this event may be called multiple times because the advertisment may be splitted into multiple packets due to the limit of 31 bytes of payload
                if (TraceDebug)
                {
                    var log = string.Format("ScanResponseEvent:" + Environment.NewLine + "\trssi={0}," + Environment.NewLine + "\tpacket_type ={1}," + Environment.NewLine + " \tbd_addr=[ {2}]," + Environment.NewLine + " \taddress_type={3}," + Environment.NewLine + "\tbond={4}, " + Environment.NewLine + "\tdata=[ {5}]" + Environment.NewLine + "\tdata raw=[{6}]" + Environment.NewLine,
                    e.Rssi,
                    (sbyte)e.PacketType,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.Address),
                    (sbyte)e.AddressType,
                    (sbyte)e.Bond,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.Data),
                    BlueGigaBLEHelper.ByteArrayToNumberString(e.Data)
                    );
                    Logger?.LogDebug(log);
                }
                //we only want to discover Lego-devices
                //filter for correct service-GUID for LEGO-Hub, containing manufacturerData and the name
                var addressNumber = BlueGigaBLEHelper.ByteArrayToUlong(e.Address);
                var bleAdvertisingListFound = bleParsedData.TryGetValue(addressNumber, out var actualListAdvertisingData);
                if (!bleAdvertisingListFound)
                {
                    actualListAdvertisingData = bleParsedData.AddOrUpdate(addressNumber, new List<BleAdvertisingData>(), (oldkey, oldvalue) => oldvalue = new List<BleAdvertisingData>());
                }
                foreach (var advData in e.ParsedData)
                {
                    actualListAdvertisingData.Add(advData);
                }
                actualListAdvertisingData = bleParsedData.AddOrUpdate(addressNumber, actualListAdvertisingData, (oldkey, oldvalue) => oldvalue = actualListAdvertisingData);
                if (actualListAdvertisingData.Any(x => x.Type == BleAdvertisingDataType.CompleteListof128BitServiceClassUUIDs && x.ToGuid() == new Guid(PoweredUpBluetoothConstants.LegoHubService))
                    && actualListAdvertisingData.Any(y => y.Type == BleAdvertisingDataType.ManufacturerSpecificData)
                    && actualListAdvertisingData.Any(z => z.Type == BleAdvertisingDataType.CompleteLocalName))
                {
                    var deviceInfo = new PoweredUpBluetoothDeviceInfo
                    {
                        BluetoothAddress = BlueGigaBLEHelper.ByteArrayToUlong(e.Address),
                        ManufacturerData = actualListAdvertisingData.First(y => y.Type == BleAdvertisingDataType.ManufacturerSpecificData).Data.Skip(2).ToArray(), //PoweredUp only wants to have data starting with the button state (see 2. advertising in [Lego]
                        Name = actualListAdvertisingData.First(z => z.Type == BleAdvertisingDataType.CompleteLocalName).ToAsciiString()
                    };
                    _ = DevicesInfo.AddOrUpdate(deviceInfo.BluetoothAddress, deviceInfo, (key, oldvalue) => oldvalue = deviceInfo);
                    actualListAdvertisingData.Clear();
                    await discoveryHandler(deviceInfo);
                }
            }
            //stop discovery first if it is running
            bleDeviceDiscovery.StopDeviceDiscovery();
            if (TraceDebug)
            {
                Logger?.LogDebug("Discovery-mode stopped explicitly on BlueGiga-Bluetoothadpater before starting new discovery...");
            }
            bleDeviceDiscovery.ScanResponse += myGAPScanResponseHandler;
            //make sure the discovery is stopped when Discover() is canceled
            _ = cancellationToken.Register(() =>
            {
                bleDeviceDiscovery.ScanResponse -= myGAPScanResponseHandler;
                bleDeviceDiscovery.StopDeviceDiscovery();
                if (TraceDebug)
                {
                    Logger?.LogDebug("Discovery-mode stopped on BlueGiga-Bluetoothadpater because Discover() has been canceled...");
                }
            });
            // begin scanning for BLE peripherals
            bleDeviceDiscovery.StartDeviceDiscovery();
        }
        /// <summary>
        /// Get the device given by the bluetooth-address (ulong) and discover all services and their characteristics. The  device is connected after this call!
        /// </summary>
        /// <param name="bluetoothAddress"></param>
        /// <returns></returns>
        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var bleDeviceManager = new BleDeviceManager(BgLib, BleModuleConnection);
            var bluetoothAdressBytes = BlueGigaBLEHelper.UlongTo6ByteArray(bluetoothAddress);
            var bleDevice = await bleDeviceManager.ConnectAsync(bluetoothAdressBytes, BleAddressType.Public);
            var name = DevicesInfo.TryGetValue(bluetoothAddress, out var myinfo) ? myinfo.Name : "NameUnknown";
            var device = new BlueGigaBLEPoweredUpBluetoothDevice(bluetoothAddress, bluetoothAdressBytes, name, this, Logger, TraceDebug, bleDevice)
            {
                IsConnected = bleDevice.IsConnected,
            };
            await Devices.AddOrUpdate(device.DeviceAdress, device, (key, oldvalue) => oldvalue = device).LogInfosAsync();
            return await Task.FromResult(Devices[bluetoothAddress]);
        }
        #endregion
        #region IDisposable
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var device in Devices)
                    {
                        //disconnecting is done in dispose of the device, because this may also need other actions in the duty of the device
                        device.Value.Dispose();
                    }
                    BleModuleConnection.Stop();
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
                        _ = sb.Append($"{header.ToUpper()}{Environment.NewLine}");
                    }
                    _ = sb.Append(await GetLogInfosAsync(indent));
                    if (!string.IsNullOrEmpty(footer))
                    {
                        _ = sb.Append($"{footer.ToUpper()}{Environment.NewLine}");
                    }
                    Logger?.LogDebug(sb.ToString());
                });
            }
        }

        public async Task<string> GetLogInfosAsync(int indent)
        {
            var stringToLog = new StringBuilder();
            _ = await Task.Run(() =>
            {
                var indentStr = new string('\t', indent < 0 ? 0 : indent);

                _ = stringToLog.Append($"{indentStr}*** Bluegiga-Adapter-Info ***:" + Environment.NewLine + $"{indentStr}Serial Port Name: {SerialAPI.PortName}" + Environment.NewLine + $"{indentStr}Serial Port Speed: {SerialAPI.BaudRate} baud" + Environment.NewLine);

                if (!Devices.IsEmpty)
                {
                    _ = stringToLog.Append($"{indentStr}I know about the following {Devices.Count} devices connected to me:{Environment.NewLine}");
                    foreach (var device in Devices)
                    {
                        _ = stringToLog.Append(device.Value.GetLogInfosAsync(indent + 1));
                    }

                    _ = stringToLog.Append($"{indentStr}End of my known devices{Environment.NewLine}");
                }
                else
                {
                    _ = stringToLog.Append($"{indentStr}I DON'T know about any devices connected to me!{Environment.NewLine}");
                }

                if (!DevicesInfo.IsEmpty)
                {
                    _ = stringToLog.Append($"{indentStr}I know about the following {Devices.Count} devices which have been found by Discovery (not neccessarily connected):{Environment.NewLine}");
                    var innerindentStr = indentStr + "\t";
                    foreach (var device in DevicesInfo)
                    {
                        _ = stringToLog.Append($"{innerindentStr}Bluetooth-Adress (ulong): {device.Value.BluetoothAddress}" + Environment.NewLine + $"{innerindentStr}Bluetooth-Name: {device.Value.Name}" + Environment.NewLine + $"{innerindentStr}Bluetooth-ManufacturerData (decimal): {BlueGigaBLEHelper.ByteArrayToNumberString(device.Value.ManufacturerData)}" + Environment.NewLine + $"{innerindentStr}Bluetooth-ManufacturerData (hex): {BlueGigaBLEHelper.ByteArrayToHexString(device.Value.ManufacturerData)}" + Environment.NewLine);
                    }
                    _ = stringToLog.Append($"{indentStr}End of devices which have been found by Discovery (not neccessarily connected){Environment.NewLine}");
                }
                else
                {
                    _ = stringToLog.Append($"{indentStr}I actually DON'T know about any devices found by Discovery!{Environment.NewLine}");
                }

                return stringToLog;
            });
            return stringToLog.ToString();
        }
        #endregion
    }
}
