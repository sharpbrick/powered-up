using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using SharpBrick.PoweredUp.Bluetooth;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Bluegiga.BLE.Events.Connection;
using Bluegiga.BLE.Events.ATTClient;
using System.Text;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEPoweredUpBluetoothAdapater : IPoweredUpBluetoothAdapter, IDisposable, IBlueGigaLogger
    {
        #region Properties
        /// <summary>
        /// during Connect and Resolve of devices/services/characteristics this status is needed for differentiate the behaviour of end_procedure-handler
        /// </summary>
        public BlueGigaBLEStatus BleStatus { get; set; }
        /// <summary>
        /// The BGLib-obejct which does the low-level communication over the BlueGiga-adapter
        /// </summary>
        public Bluegiga.BGLib Bglib { get; init; }
        /// <summary>
        /// The serial connection over which the BleuGiga-adapter communicates
        /// </summary>
        public SerialPort SerialAPI { get; init; }
        /// <summary>
        /// The ILogger-instance
        /// </summary>
        private ILogger Logger { get; init; }
        /// <summary>
        /// The devices this adapter knows about (by Discovery or direct Connect)
        /// </summary>
        private ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice> Devices { get; }
        private ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo> DevicesInfo { get; }
        //semaphore for signal successful connection
        private SemaphoreSlim Connected { get; set; } = new SemaphoreSlim(0, 1);
        //semaphore for signal finished discovery of services of a device
        private SemaphoreSlim AllServicesForDeviceDiscovered { get; set; } = new SemaphoreSlim(0, 1);
        //semaphore for signal finished discovery of characteristics of a service
        private SemaphoreSlim AllCharacteristicsForServiceDiscovered { get; set; } = new SemaphoreSlim(0, 1);
        #endregion
        #region Private fields
        private readonly byte[] legoHubServiceUuidBytes = BlueGigaBLEHelper.ConvertUUIDToByteArray(new Guid(PoweredUpBluetoothConstants.LegoHubService));
        private bool wasLastConnectSuccesfull = false;
        private byte lastConnectionHandle = 0xff;
        #endregion

        #region Constructor
        public BlueGigaBLEPoweredUpBluetoothAdapater(string comPortName, bool traceDebug, ILogger<BlueGigaBLEPoweredUpBluetoothAdapater> logger = default)
        {
            Logger = logger;
            TraceDebug = traceDebug;
            Bglib = new Bluegiga.BGLib(Logger, traceDebug);
            BleStatus = new BlueGigaBLEStatus(BlueGigaBLEStatus.StateReseting, Logger, traceDebug);
            Devices = new ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice>();
            DevicesInfo = new ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo>();
            SerialAPI = new SerialPort
            {
                Handshake = Handshake.RequestToSend,
                BaudRate = 115200,
                PortName = comPortName,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None
            };
            //this interprets the serial data as BlueGiga-Packets; see DataReceivedHandler
            SerialAPI.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            SerialAPI.Open();
            // initialize BGLib events needed for the Adapter
            //fires on reset/reboot
            Bglib.BLEEventSystemBoot += new Bluegiga.BLE.Events.System.BootEventHandler(SystemBootEventHandler);
            //fires on Connect-trial
            Bglib.BLEEventConnectionStatus += new StatusEventHandler(ConnectionStatusHandler);
            //fires on finding a service
            Bglib.BLEEventATTClientGroupFound += new GroupFoundEventHandler(ServiceFoundHandler);
            //fires on finding an attribute/characteristic
            Bglib.BLEEventATTClientFindInformationFound += new FindInformationFoundEventHandler(CharacteristicFoundHandler);
            //fires on end of service-discovery or end of characteristic-discovery
            Bglib.BLEEventATTClientProcedureCompleted += new ProcedureCompletedEventHandler(ServiceOrCharacteristicSearchCompletedHandler);
            var cmd = Bglib.BLECommandSystemReset(0x00); //boot normal, not in Device-Firmware-Update-mode!
        }
        #endregion
        #region System-/Serial-handler
        public async void SystemBootEventHandler(object sender, Bluegiga.BLE.Events.System.BootEventArgs e)
        {
            //according to documentation this event is not fired when USB-connected???
            var log = $"SystemBootEvent: {Environment.NewLine} \tmajor={e.major}{Environment.NewLine}\tminor={e.minor}{Environment.NewLine}\tpatch={e.patch}{Environment.NewLine}\tbuild={e.build}{Environment.NewLine}\tll_version={e.ll_version}{Environment.NewLine}\tprotocol_version={e.protocol_version}{Environment.NewLine}\thw={e.hw}{Environment.NewLine}";
            if (TraceDebug)
            {
                Logger?.LogDebug(log);
            }
            _ = await Task.Run(() => BleStatus.ActualStatus = BlueGigaBLEStatus.StateStandby);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            var inData = new byte[serialPort.BytesToRead];
            // read all available bytes from serial port in one chunk
            _ = serialPort.Read(inData, 0, serialPort.BytesToRead);
            // DEBUG: display bytes read
            if (TraceDebug)
            {
                Logger?.LogDebug($"Received (RX) ({inData.Length} bytes): [{BlueGigaBLEHelper.ByteArrayToHexString(inData)}]");
            }
            // parse all bytes read through BGLib parser, which fires the events and alike
            for (var i = 0; i < inData.Length; i++)
            {
                _ = Bglib.Parse(inData[i]);
            }
        }
        #endregion

        #region Interface IPoweredUpBluetoothAdapter
        /// <summary>
        /// Start scanning for BLE-advertis-packages and, when found FIRST COMPLETE advertisment from a lego-device, await the discoveryHandler.
        /// The found device DOES NOT get connected here!
        /// </summary>
        /// <param name="discoveryHandler">The handler form the SharpBrick.PoweredUp-core which shall be called when a device has been discovered</param>
        /// <param name="cancellationToken">Cancelation-token to handle a cancel of the discovery</param>
        public void Discover(Func<PoweredUpBluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            var myGAPScanResponseHandler = new Bluegiga.BLE.Events.GAP.ScanResponseEventHandler(GAPScanResponseEvent);
            // stop the scan/connect process which may be open/running
            byte[] cmd;
            cmd = Bglib.BLECommandGAPEndProcedure();
            Bglib.BLEEventGAPScanResponse -= myGAPScanResponseHandler;
            _ = Bglib.SendCommand(SerialAPI, cmd);
            if (TraceDebug)
            {
                Logger?.LogDebug("Discovery-mode stopped explicitly on BlueGiga-Bluetoothadpater...");
            }
            // set scan parameters
            Bglib.BLEEventGAPScanResponse += myGAPScanResponseHandler;
            cmd = Bglib.BLECommandGAPSetScanParameters(0xC8, 0xC8, 1); // 125ms interval, 125ms window, active scanning
            _ = Bglib.SendCommand(SerialAPI, cmd);
            //make sure the discovery is stopped when Discover() is canceled
            _ = cancellationToken.Register(() =>
            {
                //stop discovery and detach the handler when cancel of the cts is called
                Bglib.BLEEventGAPScanResponse -= myGAPScanResponseHandler;
                cmd = Bglib.BLECommandGAPEndProcedure();
                _ = Bglib.SendCommand(SerialAPI, cmd);
                if (TraceDebug)
                {
                    Logger?.LogDebug("Discovery-mode stopped on BlueGiga-Bluetoothadpater because Discover() has been canceled...");
                }
            });
            // begin scanning for BLE peripherals
            cmd = Bglib.BLECommandGAPDiscover(1); // generic discovery mode
            _ = Bglib.SendCommand(SerialAPI, cmd);
            // update state
            BleStatus.ActualStatus = BlueGigaBLEStatus.StateScanning;
            if (TraceDebug)
            {
                Logger?.LogDebug("Started scanning on BlueGiga-Bluetoothadpater...");
            }

            async void GAPScanResponseEvent(object sender, Bluegiga.BLE.Events.GAP.ScanResponseEventArgs e)
            {
                //this event may be called multiple times because the advertisment may be splitted into multiple packets due to the limit of 31 bytes of payload
                //so we have to fetch multiple times and remeber what we've received
                //we need:
                // name of the device AD-Type 9
                // manufacturer data in AD-Type FF
                // Bluetooth-Adress: Part of each AD-package's preamble
                // only if we have all information, we can go on and return the device to PowerUp-Blootoothkernel
                var log = string.Format("ScanResponseEvent:" + Environment.NewLine + "\trssi={0}," + Environment.NewLine + "\tpacket_type ={1}," + Environment.NewLine + " \tbd_addr=[ {2}]," + Environment.NewLine + " \taddress_type={3}," + Environment.NewLine + "\tbond={4}, " + Environment.NewLine + "\tdata=[ {5}]" + Environment.NewLine + "\tdata raw=[{6}]" + Environment.NewLine,
                    e.rssi,
                    (sbyte)e.packet_type,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.sender),
                    (sbyte)e.address_type,
                    (sbyte)e.bond,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.data),
                    BlueGigaBLEHelper.ByteArrayToNumberString(e.data)
                    );
                if (TraceDebug)
                {
                    Logger?.LogDebug(log);
                }
                //we only want to discover Lego-devices
                //filter for correct service-GUID for LEGO-Hub
                // pull all advertised service info from ad packet
                var advertised_services = new List<byte[]>();
                byte[] this_field = { };
                byte[] manufacturdata = { };
                byte[] deviceNameBytes = { };
                var bytes_left = 0;
                var field_offset = 0;
                for (var i = 0; i < e.data.Length; i++)
                {
                    if (bytes_left == 0)
                    {
                        bytes_left = e.data[i];
                        this_field = new byte[e.data[i]];
                        field_offset = i + 1;
                    }
                    else
                    {
                        this_field[i - field_offset] = e.data[i];
                        bytes_left--;
                        if (bytes_left == 0)
                        {
                            if (this_field[0] is 0x02 or 0x03)
                            {
                                // partial or complete list of 16-bit UUIDs
                                advertised_services.Add(this_field.Skip(1).Take(2).Reverse().ToArray());
                            }
                            else if (this_field[0] is 0x04 or 0x05)
                            {
                                // partial or complete list of 32-bit UUIDs
                                advertised_services.Add(this_field.Skip(1).Take(4).Reverse().ToArray());
                            }
                            else if (this_field[0] is 0x06 or 0x07)
                            {
                                // partial or complete list of 128-bit UUIDs
                                advertised_services.Add(this_field.Skip(1).Take(16).Reverse().ToArray());
                                manufacturdata = e.data.Skip(i + 5).ToArray();
                            }
                            else if (this_field[0] == 0x09)
                            {
                                // deviceName
                                deviceNameBytes = this_field.Skip(1).ToArray();
                            }
                        }
                    }
                }
                //Verified that Reverse is needed, because bglib works with little endian
                var deviceAdress = ulong.Parse(BlueGigaBLEHelper.ByteArrayToHexString(e.sender.Reverse().ToArray()).Replace(" ", ""), NumberStyles.HexNumber);
                var mysender = BlueGigaBLEHelper.UlongTo6ByteArray(deviceAdress);
                PoweredUpBluetoothDeviceInfo poweredUpBluetoothDeviceInfo = null;
                // check for LEGO-ServiceUUID; if found, generate a new entry in the list of LEGO-devices
                if (advertised_services.Any(a => a.SequenceEqual(legoHubServiceUuidBytes)))
                {
                    //here we have a device with the Lego-Service:
                    if (DevicesInfo.ContainsKey(deviceAdress))
                    {
                        poweredUpBluetoothDeviceInfo = DevicesInfo[deviceAdress];
                    }
                    else
                    {
                        poweredUpBluetoothDeviceInfo = new PoweredUpBluetoothDeviceInfo
                        {
                            BluetoothAddress = deviceAdress
                        };
                        if (deviceNameBytes.Length > 0)
                        {
                            poweredUpBluetoothDeviceInfo.Name = Encoding.Default.GetString(deviceNameBytes);
                        }

                        _ = DevicesInfo.AddOrUpdate(deviceAdress, poweredUpBluetoothDeviceInfo, (key, oldvalue) => oldvalue = poweredUpBluetoothDeviceInfo);
                        //poweredUpBluetoothDeviceInfo = devicesinfo[deviceAdress];
                    }
                }
                if (DevicesInfo.ContainsKey(deviceAdress))
                {
                    poweredUpBluetoothDeviceInfo = DevicesInfo[deviceAdress];
                }
                if (poweredUpBluetoothDeviceInfo != null)
                {
                    //manufacturer data
                    if (manufacturdata.Length > 0)
                    {
                        poweredUpBluetoothDeviceInfo.ManufacturerData = manufacturdata;
                    }
                    //device name
                    if (deviceNameBytes.Length > 0)
                    {
                        poweredUpBluetoothDeviceInfo.Name = Encoding.Default.GetString(deviceNameBytes);
                    }
                    //poweredUpBluetoothDeviceInfo.BluetoothAddress = deviceAdress;
                    DevicesInfo[deviceAdress] = poweredUpBluetoothDeviceInfo;
                    //all information needed? Then await the discoveryHandler (which may cancel --> unregister
                    if (DevicesInfo[deviceAdress].ManufacturerData.Length > 0 && !string.IsNullOrEmpty(DevicesInfo[deviceAdress].Name))
                    {
                        //stop discoverymode
                        var stopDiscoveryModeCmd = Bglib.BLECommandGAPEndProcedure();
                        _ = Bglib.SendCommand(SerialAPI, stopDiscoveryModeCmd);
                        BleStatus.ActualStatus = BlueGigaBLEStatus.StateStandby;
                        await discoveryHandler(DevicesInfo[deviceAdress]);
                    }
                }
            }
        }
        /// <summary>
        /// Get the device given by the bluetooth-address (ulong) and discover all services and their characteristics. The  device is connected after this call!
        /// </summary>
        /// <param name="bluetoothAddress"></param>
        /// <returns></returns>
        public async Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            var bluetoothAdressBytes = BlueGigaBLEHelper.UlongTo6ByteArray(bluetoothAddress);
            byte[] directConnectCmd;
            directConnectCmd = Bglib.BLECommandGAPConnectDirect(bluetoothAdressBytes, 0x00, 0x20, 0x30, 0x100, 0);
            BleStatus.ActualStatus = BlueGigaBLEStatus.StateConnecting;
            if (TraceDebug)
            {
                Logger?.LogDebug($"Trying to connect to [{BlueGigaBLEHelper.ByteArrayToHexString(bluetoothAdressBytes)}](HEX little endian) [{bluetoothAddress}] (ulong)");
            }
            _ = Bglib.SendCommand(SerialAPI, directConnectCmd);
            //wait until the connection has been established or errored --> wait for ConnectionstatusEvent
            wasLastConnectSuccesfull = false;
            lastConnectionHandle = 0xff;
            Connected = new SemaphoreSlim(0, 1);
            await Connected.WaitAsync();
            var name = DevicesInfo.TryGetValue(bluetoothAddress, out var myinfo) ? myinfo.Name : "NameUnknown";
            BlueGigaBLEPoweredUpBluetoothDevice device;
            device = Devices.GetOrAdd(bluetoothAddress, new BlueGigaBLEPoweredUpBluetoothDevice(bluetoothAddress, bluetoothAdressBytes, name, this, Logger, TraceDebug, lastConnectionHandle));
            device.IsConnected = wasLastConnectSuccesfull;
            wasLastConnectSuccesfull = false;
            lastConnectionHandle = 0xff;
            //now discover services and characteristics
            await device.LogInfosAsync(0, "*** DEVICE AFTER CONNECT", "*** END DEVICE AFTER CONNECT");
            if (device.IsConnected)
            {
                //discover services first
                var discoverDeviceServicesCmd = Bglib.BLECommandATTClientReadByGroupType(device.ConnectionHandle, 0x0001, 0xFFFF, new byte[] { 0x00, 0x28 }); // "service" UUID is 0x2800 for group of characteristics = service  (little-endian for UUID uint8array)
                // update state
                BleStatus.ActualStatus = BlueGigaBLEStatus.StateFindingServices;
                _ = Bglib.SendCommand(SerialAPI, discoverDeviceServicesCmd);
                //wait until all services of the device have been discovered
                await AllServicesForDeviceDiscovered.WaitAsync();
                await device.LogInfosAsync(0, "*** DEVICE AFTER ALL SERVICES DISCOVERED (NO CHARACTERISTICS YET)", "*** END DEVICE AFTER ALL SERVICES DISCOVERED (NO CHARACTERISTICS YET)");
                _ = Devices.AddOrUpdate(device.DeviceAdress, device, (key, oldvalue) => oldvalue = device);
                //now discover characteristics of all services
                if (device.GATTServices?.Count > 0)
                {
                    BleStatus.ActualStatus = BlueGigaBLEStatus.StateFindingattributes;
                    foreach (var service in device.GATTServices)
                    {
                        var discoverCharacteristicsCmd = Bglib.BLECommandATTClientFindInformation(service.Value.Connection, service.Value.FirstCharacterHandle, service.Value.LastCharacterHandle);
                        _ = Bglib.SendCommand(SerialAPI, discoverCharacteristicsCmd);
                        await AllCharacteristicsForServiceDiscovered.WaitAsync();
                    }
                }
                BleStatus.ActualStatus = BlueGigaBLEStatus.StateStandby;
                await device.LogInfosAsync(0, "*** DEVICE AFTER ALL SERVICES AND CHARACTERISTICS DISCOVERED", "*** END AFTER ALL SERVICES AND CHARACTERISTICS DISCOVERED");
            }
            _ = Devices.AddOrUpdate(device.DeviceAdress, device, (key, oldvalue) => oldvalue = device);
            return await Task.FromResult(Devices[bluetoothAddress]);
        }
        #endregion

        #region Event-Handler for connect and service-/characteristic discovery
        /// <summary>
        /// This eventhandler is called when a connect-attempt is acknowleged by the device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectionStatusHandler(object sender, StatusEventArgs e)
        {
            if (TraceDebug)
            {
                await Task.Run(() =>
                {
                    var log = "ConnectionStatusHandler:" + Environment.NewLine
                        + $"\tConnection-Handle: {e.connection}," + Environment.NewLine
                        + $"\tFlags: {Convert.ToString(e.flags, 2).PadLeft(8, '0')}," + Environment.NewLine
                        + $"\tAdressBytes (little endian): {e.address}," + Environment.NewLine
                        + $"\tAdress (Hex): {BlueGigaBLEHelper.ByteArrayToHexString(e.address)}," + Environment.NewLine
                        + $"\tAdressType: {e.address_type}," + Environment.NewLine
                        + $"\tConnection-Interval: {e.conn_interval} (= {e.conn_interval * 1.25}ms)," + Environment.NewLine
                        + $"\tTimeout: {e.timeout} (={e.timeout * 10}ms)," + Environment.NewLine
                        + $"\tLatency: {e.latency}" + Environment.NewLine
                        + $"\tBonding: {e.bonding}";
                    Logger?.LogDebug(log);
                });
            }
            if ((e.flags & 0x05) == 0x05)
            {
                wasLastConnectSuccesfull = true;
                lastConnectionHandle = e.connection;
            }
            else
            {
                wasLastConnectSuccesfull = false;
                lastConnectionHandle = 0xff;
            }
            _ = await Task.Run(() => BleStatus.ActualStatus = BlueGigaBLEStatus.StateStandby);
            _ = Connected.Release();
        }

        /// <summary>
        /// This handler is called every time the device sends a service-information (on request of the client); called for each service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ServiceFoundHandler(object sender, GroupFoundEventArgs e)
        {
            //add the service to the device's service-collection by generating a new Service-object
            //identify the device by the connection-byte used on the adapter for talking to this device and then add the service to this device
            var serviceUuid = BlueGigaBLEHelper.ConvertByteArrayToGuid(e.uuid);
            try
            {
                await Task.Run(() =>
                {
                    var mydevice = Devices.First(d => d.Value.ConnectionHandle == e.connection).Value;
                    BlueGigaBLEPoweredUpBluetoothService myservice;
                    myservice = new BlueGigaBLEPoweredUpBluetoothService(mydevice, serviceUuid, e.start, e.end, Logger, TraceDebug);
                    _ = mydevice.GATTServices.AddOrUpdate(serviceUuid, myservice, (guid, service) => service = myservice);
                });
            }
            catch (Exception ex)
            {
                if (TraceDebug)
                {
                    Logger?.LogError($"Cannot add the service with Service-UUID {serviceUuid} to the service-collection of the device with the connection-handle {e.connection} ! Error:{ex.Message} ");
                }
            }
            if (TraceDebug)
            {
                Logger?.LogDebug($"Service found (ServiceFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
            }
            if (TraceDebug)
            {
                Logger?.LogDebug($"Service found (ServiceFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
            }
        }

        /// <summary>
        /// This handler is called every time the device sends a characteristic-information (on request of the client); called for each characteristic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CharacteristicFoundHandler(object sender, FindInformationFoundEventArgs e)
        {
            //Add the characteristic to the service
            //not sure how to identify the service to which this characteristic is belonging to??
            //by e.connection --> device --> service and e.chrhandle in service.starthandle..service.endhandle
            var characteristicUuid = BlueGigaBLEHelper.ConvertByteArrayToGuid(e.uuid);
            try
            {
                await Task.Run(() =>
                {
                    var mydevice = Devices.First(d => d.Value.ConnectionHandle == e.connection).Value;
                    var myservice = mydevice.GATTServices.First(s => (s.Value.FirstCharacterHandle <= e.chrhandle) && (s.Value.LastCharacterHandle >= e.chrhandle)).Value;
                    var mycharacteristic = new BlueGigaBLEPoweredUpBluetoothCharacteristic(myservice, e.chrhandle, characteristicUuid, Logger, TraceDebug);
                    _ = myservice.GATTCharacteristics.AddOrUpdate(characteristicUuid, mycharacteristic, (guid, characteristic) => characteristic = mycharacteristic);
                });
            }
            catch (Exception ex)
            {
                if (TraceDebug)
                {
                    Logger?.LogError($"Cannot add the characteristic with Characteristic-UUID {characteristicUuid} to the charcteristic-collection of the service with the connection-handle {e.connection}. Handle of the characteristic: {e.chrhandle} ! Error:{ex.Message} ");
                }
            }
            if (TraceDebug)
            {
                Logger?.LogDebug($"Characteristic found (CharacteristicFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
            }
            if (TraceDebug)
            {
                Logger?.LogDebug($"Characteristic found (CharacteristicFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
            }
        }

        /// <summary>
        /// This event is fired when either the service-finding or the characteristic-finding ends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ServiceOrCharacteristicSearchCompletedHandler(object sender, ProcedureCompletedEventArgs e)
        {
            await Task.Run(() =>
            {
                //signal end of dicovery when all services for a device have been discovered
                if (BleStatus.ActualStatus == BlueGigaBLEStatus.StateFindingServices)
                {
                    _ = AllServicesForDeviceDiscovered.Release();
                }
                //signal end of dicovery when all characteristics for a device have been discovered
                if (BleStatus.ActualStatus == BlueGigaBLEStatus.StateFindingattributes)
                {
                    _ = AllCharacteristicsForServiceDiscovered.Release();
                }
            });
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
            var stringToLog = new StringBuilder();
            _ = await Task.Run(() =>
            {
                var indentStr = new string('\t', indent < 0 ? 0 : indent);

                _ = stringToLog.Append($"{indentStr}*** Bluegiga-Adapter-Info ***:" + Environment.NewLine + $"{indentStr}Serial Port Name: {SerialAPI.PortName}" + Environment.NewLine + $"{indentStr}Serial Port Speed: {SerialAPI.BaudRate} baud" + Environment.NewLine + $"{indentStr}Actual Status (BlueGigaBLEStatus): {BleStatus}" + Environment.NewLine);

                if (Devices.Count > 0)
                {
                    _ = stringToLog.Append($"{indentStr}I know about the following {Devices.Count} devices connected to me:");
                    foreach (var device in Devices)
                    {
                        _ = stringToLog.Append(device.Value.GetLogInfosAsync(indent + 1));
                    }

                    _ = stringToLog.Append($"{indentStr}End of my known devices");
                }
                else
                {
                    _ = stringToLog.Append($"{indentStr}I DON'T know about any devices connected to me!");
                }

                if (DevicesInfo.Count > 0)
                {
                    _ = stringToLog.Append($"{indentStr}I know about the following {Devices.Count} devices which have been found by Discovery (not neccessarily connected):");
                    var innerindentStr = indentStr + "\t";
                    foreach (var device in DevicesInfo)
                    {
                        _ = stringToLog.Append($"{innerindentStr}Bluetooth-Adress (ulong): {device.Value.BluetoothAddress}" + Environment.NewLine + $"{innerindentStr}Bluetooth-Name: {device.Value.Name}" + Environment.NewLine + $"{innerindentStr}Bluetooth-ManufacturerData (decimal): {BlueGigaBLEHelper.ByteArrayToNumberString(device.Value.ManufacturerData)}" + Environment.NewLine + $"{innerindentStr}Bluetooth-ManufacturerData (hex): {BlueGigaBLEHelper.ByteArrayToHexString(device.Value.ManufacturerData)}" + Environment.NewLine);
                    }
                    _ = stringToLog.Append($"{indentStr}End of devices which have been found by Discovery (not neccessarily connected)");
                }
                else
                {
                    _ = stringToLog.Append($"{indentStr}I actually DON'T know about any devices found by Discovery!");
                }

                return stringToLog;
            });
            return stringToLog.ToString();
        }
        #endregion
    }
}
