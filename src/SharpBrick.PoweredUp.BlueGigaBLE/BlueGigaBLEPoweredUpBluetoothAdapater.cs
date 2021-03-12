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
using Bluegiga.BLE.Responses.ATTClient;
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
        public Bluegiga.BGLib Bglib { get; set; }
        /// <summary>
        /// The serial connection over which the BleuGiga-adapter communicates
        /// </summary>
        public SerialPort SerialAPI { get; set; }
        /// <summary>
        /// The ILogger-instance
        /// </summary>
        public ILogger _logger { get; set; }
        /// <summary>
        /// The devices this adapter knows about (by Discovery or direct Connect)
        /// </summary>
        public ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice> myDevices;
        #endregion
        #region Private Variables
        private ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo> devicesinfo;
        private byte[] legoHubServiceBytes;
        //semaphore for signal successful connection
        private SemaphoreSlim connected;
        //semaphore for signal finished discovery of services of a device
        private SemaphoreSlim allServicesForDeviceDiscovered = new SemaphoreSlim(0, 1);
        //semaphore for signal finished discovery of characteristics of a service
        private SemaphoreSlim allCharacteristicsForServiceDiscovered = new SemaphoreSlim(0, 1);
        private bool wasLastConnectSuccesfull = false;
        private byte lastConnectionHandle = 0xff;
        #endregion

        #region Constructor
        public BlueGigaBLEPoweredUpBluetoothAdapater(String comPortName ,  bool traceDebug, ILogger<SharpBrick.PoweredUp.BlueGigaBLE.BlueGigaBLEPoweredUpBluetoothAdapater> logger = default)
        {
            _logger = logger;
            TraceDebug = traceDebug;
            Bglib = new Bluegiga.BGLib(_logger , traceDebug);
            BleStatus = new BlueGigaBLEStatus(BlueGigaBLEStatus.STATE_RESETING, _logger, traceDebug);
            legoHubServiceBytes = BlueGigaBLEHelper.ConvertUUIDToByteArray(new Guid(PoweredUpBluetoothConstants.LegoHubService));
            myDevices = new ConcurrentDictionary<ulong, BlueGigaBLEPoweredUpBluetoothDevice>();
            devicesinfo = new ConcurrentDictionary<ulong, PoweredUpBluetoothDeviceInfo>();
            //TODO: .NET 5 read the portname from configuration/serviceoptions/parameters /see ServiceCollectionExtensionsForBlueGigaBLE.cs
            InitSerial(comPortName, 9600);
            // initialize BGLib events needed for the Adapter
            //fires on reset/reboot
            Bglib.BLEEventSystemBoot += new Bluegiga.BLE.Events.System.BootEventHandler(this.SystemBootEventHandler);
            //fires on Connect-trial
            Bglib.BLEEventConnectionStatus += new Bluegiga.BLE.Events.Connection.StatusEventHandler(ConnectionStatusHandler);
            //fires on finding a service
            Bglib.BLEEventATTClientGroupFound += new Bluegiga.BLE.Events.ATTClient.GroupFoundEventHandler(ServiceFoundHandler);
            
            //fires on finding an attribute/characteristic
            Bglib.BLEEventATTClientFindInformationFound += new Bluegiga.BLE.Events.ATTClient.FindInformationFoundEventHandler(CharacteristicFoundHandler);
            //fires on end of service-discovery or end of characteristic-discovery
            Bglib.BLEEventATTClientProcedureCompleted += new Bluegiga.BLE.Events.ATTClient.ProcedureCompletedEventHandler(ServiceOrCharacteristicSearchCompletedHandler);
            Byte[] cmd = Bglib.BLECommandSystemReset(0x00); //boot normal, not in Device-Firmware-Update-mode!
        }
        #endregion
        #region Initialization and System-/Serial-handler
        private void InitSerial(String portname, int baudrate)
        {
            this.SerialAPI = new System.IO.Ports.SerialPort();
            SerialAPI.Handshake = System.IO.Ports.Handshake.RequestToSend;
            SerialAPI.BaudRate = baudrate;
            SerialAPI.PortName = portname;
            SerialAPI.DataBits = 8;
            SerialAPI.StopBits = System.IO.Ports.StopBits.One;
            SerialAPI.Parity = System.IO.Ports.Parity.None;
            //this interprets the serial data as BlueGiga-Packets; see DataReceivedHandler
            SerialAPI.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(DataReceivedHandler);
            SerialAPI.Open();
        }
        public async void SystemBootEventHandler(object sender, Bluegiga.BLE.Events.System.BootEventArgs e)
        {
            //according to documentation this event is not fired when USB-connected???
            String log = String.Format("SystemBootEvent:" + Environment.NewLine + "\tmajor={0}" + Environment.NewLine +"\tminor={1}" + Environment.NewLine +"\tpatch={2}" + Environment.NewLine +"\tbuild={3}" + Environment.NewLine +"\tll_version={4}" + Environment.NewLine +"\tprotocol_version={5}" + Environment.NewLine +"\thw={6}" + Environment.NewLine,
                e.major,
                e.minor,
                e.patch,
                e.build,
                e.ll_version,
                e.protocol_version,
                e.hw
                );
            if (TraceDebug) _logger?.LogDebug(log);
            BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_STANDBY;
        }

        private void DataReceivedHandler(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = (System.IO.Ports.SerialPort)sender;
            Byte[] inData = new Byte[sp.BytesToRead];
            // read all available bytes from serial port in one chunk
            sp.Read(inData, 0, sp.BytesToRead);
            // DEBUG: display bytes read
            if (TraceDebug) _logger?.LogDebug($"Received (RX) ({inData.Length} bytes): [{BlueGigaBLEHelper.ByteArrayToHexString(inData)}]");
            // parse all bytes read through BGLib parser, which fires the events and alike
            for (int i = 0; i < inData.Length; i++)
            {
                Bglib.Parse(inData[i]);
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
            Byte[] cmd;
            cmd = Bglib.BLECommandGAPEndProcedure();
            Bglib.BLEEventGAPScanResponse -= myGAPScanResponseHandler;
            Bglib.SendCommand(SerialAPI, cmd);
            if (TraceDebug) _logger?.LogDebug("Discovery-mode stopped explicitly on BlueGiga-Bluetoothadpater...");
            // set scan parameters
            Bglib.BLEEventGAPScanResponse += myGAPScanResponseHandler;
            cmd = Bglib.BLECommandGAPSetScanParameters(0xC8, 0xC8, 1); // 125ms interval, 125ms window, active scanning
            Bglib.SendCommand(SerialAPI, cmd);
            //make sure the discovery is stopped when Discover() is canceled
            cancellationToken.Register(() =>
            {
                //stop discovery and detach the handler when cancel of the cts is called
                Bglib.BLEEventGAPScanResponse -= myGAPScanResponseHandler;
                cmd = Bglib.BLECommandGAPEndProcedure();
                Bglib.SendCommand(SerialAPI, cmd);
                if (TraceDebug) _logger?.LogDebug("Discovery-mode stopped on BlueGiga-Bluetoothadpater because Discover() has been canceled...");
            });
            // begin scanning for BLE peripherals
            cmd = Bglib.BLECommandGAPDiscover(1); // generic discovery mode
            Bglib.SendCommand(SerialAPI, cmd);
            // update state
            BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_SCANNING;
            if (TraceDebug) _logger?.LogDebug("Started scanning on BlueGiga-Bluetoothadpater...");

            async void GAPScanResponseEvent(object sender, Bluegiga.BLE.Events.GAP.ScanResponseEventArgs e)
            {
                //this event may be called multiple times because the advertisment may be splitted into multiple packets due to the limit of 31 bytes of payload
                //so we have to fetch multiple times and remeber what we've received
                //we need:
                // name of the device AD-Type 9
                // manufacturer data in AD-Type FF
                // Bluetooth-Adress: Part of each AD-package's preamble
                // only if we have all information, we can go on and return the device to PowerUp-Blootoothkernel
                String log = String.Format("ScanResponseEvent:" + Environment.NewLine + "\trssi={0}," + Environment.NewLine + "\tpacket_type ={1}," + Environment.NewLine + " \tbd_addr=[ {2}]," + Environment.NewLine + " \taddress_type={3}," + Environment.NewLine + "\tbond={4}, " + Environment.NewLine + "\tdata=[ {5}]" + Environment.NewLine + "\tdata raw=[{6}]" + Environment.NewLine,
                    (SByte)e.rssi,
                    (SByte)e.packet_type,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.sender),
                    (SByte)e.address_type,
                    (SByte)e.bond,
                    BlueGigaBLEHelper.ByteArrayToHexString(e.data),
                    BlueGigaBLEHelper.ByteArrayToNumberString(e.data)
                    );
                if (TraceDebug) _logger?.LogDebug(log);
                //we only want to discover Lego-devices
                //filter for correct service-GUID for LEGO-Hub
                // pull all advertised service info from ad packet
                List<Byte[]> advertised_services = new List<Byte[]>();
                Byte[] this_field = { };
                Byte[] manufacturdata = { };
                Byte[] deviceNameBytes = { };
                int bytes_left = 0;
                int field_offset = 0;
                for (int i = 0; i < e.data.Length; i++)
                {
                    if (bytes_left == 0)
                    {
                        bytes_left = e.data[i];
                        this_field = new Byte[e.data[i]];
                        field_offset = i + 1;
                    }
                    else
                    {
                        this_field[i - field_offset] = e.data[i];
                        bytes_left--;
                        if (bytes_left == 0)
                        {
                            if (this_field[0] == 0x02 || this_field[0] == 0x03)
                            {
                                // partial or complete list of 16-bit UUIDs
                                advertised_services.Add(this_field.Skip(1).Take(2).Reverse().ToArray());
                            }
                            else if (this_field[0] == 0x04 || this_field[0] == 0x05)
                            {
                                // partial or complete list of 32-bit UUIDs
                                advertised_services.Add(this_field.Skip(1).Take(4).Reverse().ToArray());
                            }
                            else if (this_field[0] == 0x06 || this_field[0] == 0x07)
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
                ulong deviceAdress = ulong.Parse(BlueGigaBLEHelper.ByteArrayToHexString(e.sender.Reverse().ToArray()).Replace(" ", ""), NumberStyles.HexNumber);
                byte[] mysender = BlueGigaBLEHelper.UlongTo6ByteArray(deviceAdress);
                PoweredUpBluetoothDeviceInfo poweredUpBluetoothDeviceInfo = null;
                // check for LEGO-ServiceUUID; if found, generate a new entry in the list of LEGO-devices
                if (advertised_services.Any(a => a.SequenceEqual(legoHubServiceBytes)))
                {
                    //here we have a device with the Lego-Service:
                    if (devicesinfo.ContainsKey(deviceAdress))
                        poweredUpBluetoothDeviceInfo = devicesinfo[deviceAdress];
                    else
                    {
                        poweredUpBluetoothDeviceInfo = new PoweredUpBluetoothDeviceInfo();
                        poweredUpBluetoothDeviceInfo.BluetoothAddress = deviceAdress;
                        if (deviceNameBytes.Length > 0)
                            poweredUpBluetoothDeviceInfo.Name = System.Text.Encoding.Default.GetString(deviceNameBytes);
                        devicesinfo.AddOrUpdate(deviceAdress, poweredUpBluetoothDeviceInfo, (key, oldvalue) => oldvalue = poweredUpBluetoothDeviceInfo);
                        //poweredUpBluetoothDeviceInfo = devicesinfo[deviceAdress];
                    }
                }
                if (devicesinfo.ContainsKey(deviceAdress))
                    poweredUpBluetoothDeviceInfo = devicesinfo[deviceAdress];
                if (poweredUpBluetoothDeviceInfo != null)
                {
                    //manufacturer data
                    if (manufacturdata.Length > 0)
                        poweredUpBluetoothDeviceInfo.ManufacturerData = manufacturdata;
                    //device name
                    if (deviceNameBytes.Length > 0)
                        poweredUpBluetoothDeviceInfo.Name = System.Text.Encoding.Default.GetString(deviceNameBytes);
                    //poweredUpBluetoothDeviceInfo.BluetoothAddress = deviceAdress;
                    devicesinfo[deviceAdress] = poweredUpBluetoothDeviceInfo;
                    //all information needed? Then await the discoveryHandler (which may cancel --> unregister
                    if (devicesinfo[deviceAdress].ManufacturerData.Length > 0 && !String.IsNullOrEmpty(devicesinfo[deviceAdress].Name))
                    {
                        //stop discoverymode
                        Byte[] stopDiscoveryModeCmd = Bglib.BLECommandGAPEndProcedure();
                        Bglib.SendCommand(SerialAPI, stopDiscoveryModeCmd);
                        BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_STANDBY;
                        await discoveryHandler(devicesinfo[deviceAdress]);
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
            Byte[] bluetoothAdressBytes = BlueGigaBLEHelper.UlongTo6ByteArray(bluetoothAddress);
            Byte[] directConnectCmd;
            directConnectCmd = Bglib.BLECommandGAPConnectDirect(bluetoothAdressBytes, 0x00, 0x20, 0x30, 0x100, 0);
            BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_CONNECTING;
            if (TraceDebug) _logger?.LogDebug($"Trying to connect to [{BlueGigaBLEHelper.ByteArrayToHexString(bluetoothAdressBytes)}](HEX little endian) [{bluetoothAddress}] (ulong)");
            Bglib.SendCommand(SerialAPI, directConnectCmd);
            //wait until the connection has been established or errored --> wait for ConnectionstatusEvent
            wasLastConnectSuccesfull = false;
            lastConnectionHandle = 0xff;
            connected = new SemaphoreSlim(0, 1);
            await connected.WaitAsync();
            PoweredUpBluetoothDeviceInfo myinfo;
            String name;
            if (devicesinfo.TryGetValue(bluetoothAddress, out myinfo))
                name = myinfo.Name;
            else
                name = "NameUnknown";
            BlueGigaBLEPoweredUpBluetoothDevice device;
            device = this.myDevices.GetOrAdd(bluetoothAddress, new BlueGigaBLEPoweredUpBluetoothDevice(bluetoothAddress, bluetoothAdressBytes, name, this, _logger, TraceDebug));
            device.IsConnected = wasLastConnectSuccesfull;
            device.ConnectionHandle = lastConnectionHandle;
            wasLastConnectSuccesfull = false;
            lastConnectionHandle = 0xff;
            //now discover services and characteristics
            device.LogMyInfos(0, "*** DEVICE AFTER CONNECT" , "*** END DEVICE AFTER CONNECT");
            if (device.IsConnected)
            {
                //discover services first
                Byte[] discoverDeviceServicesCmd = Bglib.BLECommandATTClientReadByGroupType(device.ConnectionHandle, 0x0001, 0xFFFF, new Byte[] { 0x00, 0x28 }); // "service" UUID is 0x2800 for group of characteristics = service  (little-endian for UUID uint8array)
                // update state
                BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_FINDING_SERVICES;
                Bglib.SendCommand(SerialAPI, discoverDeviceServicesCmd);
                //wait until all services of the device have been discovered
                await allServicesForDeviceDiscovered.WaitAsync();
                device.LogMyInfos(0, "*** DEVICE AFTER ALL SERVICES DISCOVERED (NO CHARACTERISTICS YET)", "*** END DEVICE AFTER ALL SERVICES DISCOVERED (NO CHARACTERISTICS YET)");
                this.myDevices.AddOrUpdate(device.DeviceAdress, device, (key, oldvalue) => oldvalue = device);
                //now discover characteristics of all services
                if (device.myServices?.Count > 0)
                {
                    BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_FINDING_ATTRIBUTES;
                    foreach (KeyValuePair<Guid, BlueGigaBLEPoweredUpBluetoothService> service in device.myServices)
                    {
                        Byte[] discoverCharacteristicsCmd = Bglib.BLECommandATTClientFindInformation(service.Value.connection, service.Value.firstCharacterHandle, service.Value.lastCharacterHandle);
                        Bglib.SendCommand(SerialAPI, discoverCharacteristicsCmd);
                        await allCharacteristicsForServiceDiscovered.WaitAsync();
                    }
                }
                BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_STANDBY;
                device.LogMyInfos(0, "*** DEVICE AFTER ALL SERVICES AND CHARACTERISTICS DISCOVERED", "*** END AFTER ALL SERVICES AND CHARACTERISTICS DISCOVERED");
            }
            this.myDevices.AddOrUpdate(device.DeviceAdress, device, (key, oldvalue) => oldvalue = device);
            return await Task.FromResult(this.myDevices[bluetoothAddress]);
        }
        #endregion

        #region Event-Handler for connect and service-/characteristic discovery
        /// <summary>
        /// This eventhandler is called when a connect-attempt is acknowleged by the device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void ConnectionStatusHandler(object sender, StatusEventArgs e)
        {
            String log = "ConnectionStatusHandler:" + Environment.NewLine
                + $"\tConnection-Handle: {e.connection}," + Environment.NewLine
                + $"\tFlags: {Convert.ToString(e.flags, 2).PadLeft(8, '0')}," + Environment.NewLine
                + $"\tAdressBytes (little endian): {e.address}," + Environment.NewLine
                + $"\tAdress (Hex): {BlueGigaBLEHelper.ByteArrayToHexString(e.address)}," + Environment.NewLine
                + $"\tAdressType: {e.address_type}," + Environment.NewLine
                + $"\tConnection-Interval: {e.conn_interval} (= {e.conn_interval * 1.25}ms)," + Environment.NewLine
                + $"\tTimeout: {e.timeout} (={e.timeout * 10}ms)," + Environment.NewLine
                + $"\tLatency: {e.latency}" + Environment.NewLine
                + $"\tBonding: {e.bonding}";
            if (TraceDebug)  _logger?.LogDebug(log);
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
            BleStatus.ActualStatus = BlueGigaBLEStatus.STATE_STANDBY;
            connected.Release();
        }

        /// <summary>
        /// This handler is called every time the device sends a service-information (on request of the client); called for each service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void ServiceFoundHandler(object sender, Bluegiga.BLE.Events.ATTClient.GroupFoundEventArgs e)
        {
            //add the service to the device's service-collection by generating a new Service-object
            //identify the device by the connection-byte used on the adapter for talking to this device and then add the service to this device
            Guid serviceUuid = BlueGigaBLEHelper.ConvertByteArrayToGuid(e.uuid);
            try
            {
                BlueGigaBLEPoweredUpBluetoothDevice mydevice = this.myDevices.First(d => d.Value.ConnectionHandle == e.connection).Value;
                BlueGigaBLEPoweredUpBluetoothService myservice;
                myservice = new BlueGigaBLEPoweredUpBluetoothService(mydevice, serviceUuid, e.start, e.end, _logger, TraceDebug);
                mydevice.myServices.AddOrUpdate(serviceUuid, myservice, (guid , service)=> service = myservice);

            }
            catch (Exception ex)
            {
                if (TraceDebug) _logger?.LogError($"Cannot add the service with Service-UUID {serviceUuid} to the service-collection of the device with the connection-handle {e.connection} ! Error:{ex.Message} ");
            }
            if (TraceDebug) _logger?.LogDebug($"Service found (ServiceFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
            
            
        }

        /// <summary>
        /// This handler is called every time the device sends a characteristic-information (on request of the client); called for each characteristic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void CharacteristicFoundHandler(object sender, FindInformationFoundEventArgs e)
        {
            //Add the characteristic to the service
            //not sure how to identify the service to which this characteristic is belonging to??
            //by e.connection --> device --> service and e.chrhandle in service.starthandle..service.endhandle
            Guid characteristicUuid = BlueGigaBLEHelper.ConvertByteArrayToGuid(e.uuid);
            try
            {
                BlueGigaBLEPoweredUpBluetoothDevice mydevice = this.myDevices.First(d => d.Value.ConnectionHandle == e.connection).Value;
                BlueGigaBLEPoweredUpBluetoothService myservice = mydevice.myServices.First(s => ((s.Value.firstCharacterHandle <= e.chrhandle) && (s.Value.lastCharacterHandle >= e.chrhandle))).Value;
                BlueGigaBLEPoweredUpBluetoothCharacteristic mycharacteristic = new BlueGigaBLEPoweredUpBluetoothCharacteristic(myservice, e.chrhandle, characteristicUuid, _logger, TraceDebug);
                myservice.myCharacteristics.AddOrUpdate(characteristicUuid, mycharacteristic, (guid, characteristic) => characteristic = mycharacteristic);
            }
            catch (Exception ex)
            {
                if (TraceDebug) _logger?.LogError($"Cannot add the characteristic with Characteristic-UUID {characteristicUuid} to the charcteristic-collection of the service with the connection-handle {e.connection}. Handle of the characteristic: {e.chrhandle} ! Error:{ex.Message} ");
            }
            if (TraceDebug) _logger?.LogDebug($"Characteristic found (CharacteristicFoundHandler) with uuid-Byte-Array: [{BlueGigaBLEHelper.ByteArrayToHexString(e.uuid)}]");
        }

        /// <summary>
        /// This event is fired when either the service-finding or the characteristic-finding ends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void ServiceOrCharacteristicSearchCompletedHandler(object sender, ProcedureCompletedEventArgs e)
        {
            //signal end of dicovery when all services for a device have been discovered
            if (BleStatus.ActualStatus == BlueGigaBLEStatus.STATE_FINDING_SERVICES)
                allServicesForDeviceDiscovered.Release();
            //signal end of dicovery when all characteristics for a device have been discovered
            if (BleStatus.ActualStatus == BlueGigaBLEStatus.STATE_FINDING_ATTRIBUTES)
                allCharacteristicsForServiceDiscovered.Release();
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
                    foreach (KeyValuePair<ulong, BlueGigaBLEPoweredUpBluetoothDevice> device in this.myDevices)
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
        public bool TraceDebug { get ; set ; }
        public void LogMyInfos(int indent = 0, String header="", String footer="")
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
            StringBuilder stringToLog = new StringBuilder();
            stringToLog.Append(
                $"{indentStr}*** Bluegiga-Adapter-Info ***:" + Environment.NewLine +
                $"{indentStr}Serial Port Name: {this.SerialAPI.PortName}" + Environment.NewLine +
                $"{indentStr}Serial Port Speed: { this.SerialAPI.BaudRate} baud" + Environment.NewLine +
                $"{indentStr}Actual Status (BlueGigaBLEStatus): {this.BleStatus}" + Environment.NewLine);
            
            if (this.myDevices.Count > 0)
            {
                stringToLog.Append($"{indentStr}I know about the following {this.myDevices.Count} devices connected to me:");
                foreach (KeyValuePair<ulong, BlueGigaBLEPoweredUpBluetoothDevice> device in this.myDevices)
                   stringToLog.Append(device.Value.GetMyLogInfos(indent + 1));
                stringToLog.Append($"{indentStr}End of my known devices");
            }
            else
                stringToLog.Append($"{indentStr}I DON'T know about any devices connected to me!");
            if (this.devicesinfo.Count > 0)
            {
                stringToLog.Append($"{indentStr}I know about the following {this.myDevices.Count} devices which have been found by Discovery (not neccessarily connected):");
                String innerindentStr = indentStr + "\t";
                foreach (KeyValuePair<ulong, PoweredUpBluetoothDeviceInfo> device in this.devicesinfo)
                {
                    stringToLog.Append(
                        $"{innerindentStr}Bluetooth-Adress (ulong): {device.Value.BluetoothAddress}" + Environment.NewLine +
                        $"{innerindentStr}Bluetooth-Name: {device.Value.Name}" + Environment.NewLine +
                        $"{innerindentStr}Bluetooth-ManufacturerData (decimal): {BlueGigaBLEHelper.ByteArrayToNumberString(device.Value.ManufacturerData)}" + Environment.NewLine +
                        $"{innerindentStr}Bluetooth-ManufacturerData (hex): {BlueGigaBLEHelper.ByteArrayToHexString(device.Value.ManufacturerData)}" + Environment.NewLine);
                }
                stringToLog.Append($"{indentStr}End of devices which have been found by Discovery (not neccessarily connected)");
            }
            else
                stringToLog.Append($"{indentStr}I actually DON'T know about any devices found by Discovery!");
            return stringToLog.ToString();
        }
        #endregion
    }
}
