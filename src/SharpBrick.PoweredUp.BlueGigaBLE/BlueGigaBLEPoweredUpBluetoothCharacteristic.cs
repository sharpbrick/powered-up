using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluegiga.BLE.Events.ATTClient;
using Bluegiga.BLE.Events.Attributes;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.BlueGigaBLE
{
    public class BlueGigaBLEPoweredUpBluetoothCharacteristic : IPoweredUpBluetoothCharacteristic, IDisposable, IBlueGigaLogger
    {
        #region Properties
        /// <summary>
        /// The Service this Characteristic belongs to; will be set in the calling/constructing Service
        /// </summary>
        public BlueGigaBLEPoweredUpBluetoothService myService { get; set; }
        /// <summary>
        /// The connectionof the device offering this characteristic
        /// </summary>
        public byte connection { get { return myService.myDevice.ConnectionHandle; } }
        /// <summary>
        /// The characteristic-handle on the connection which has to be used by the bluegiga-adapter to adress this character
        /// </summary>
        public ushort myhandle { get; set; }
        /// <summary>
        /// The Logger for this object
        /// </summary>
        public readonly ILogger _logger;
        #endregion
        #region Constructors
        public BlueGigaBLEPoweredUpBluetoothCharacteristic(BlueGigaBLEPoweredUpBluetoothService myService, ushort myhandle, Guid characteristicUuid, ILogger logger, bool traceDebug)
        {
            this.myService = myService ?? throw new ArgumentNullException(nameof(myService));
            this.myhandle = myhandle;
            Uuid = characteristicUuid;
            this._logger = logger;
            this.TraceDebug = traceDebug;
        }
        #endregion
        #region IPoweredUpBlueToothCharacteristic
        /// <summary>
        /// The UUID of the characteristic
        /// </summary>
        public Guid Uuid { get; }
        public async Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
        {
            if (notificationHandler is null)
            {
                throw new ArgumentNullException(nameof(notificationHandler));
            }
            this.myService.myDevice.Bglib.BLEEventATTClientAttributeValue += new Bluegiga.BLE.Events.ATTClient.AttributeValueEventHandler(AttributeValueChangedEventHandler);
            void AttributeValueChangedEventHandler(object sender, AttributeValueEventArgs e)
            {
                
                if ((e.atthandle == this.myhandle) && (e.connection == this.connection))
                {
                    notificationHandler( e.value);
                }
            }
            Byte[] cmd = this.myService.myDevice.Bglib.BLECommandATTClientWriteCommand(connection , 0x0F , new byte[] { 0x01, 0x00 });
            var status = await Task.Run( () => this.myService.myDevice.Bglib.SendCommand( this.myService.myDevice.myBluetoothAdapter.SerialAPI,  cmd));
            return (status >= 0);
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            Byte[] cmd = this.myService.myDevice.Bglib.BLECommandATTClientWriteCommand(this.myService.connection, this.myhandle, data);
            var status = await Task.Run(() => this.myService.myDevice.Bglib.SendCommand(this.myService.myDevice.myBluetoothAdapter.SerialAPI, cmd));
            return (status==0);
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
                    // TODO: dispose managed state (managed objects)
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BlueGigaBLEPoweredUpBluetoothCharacteristic()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

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
            sb.Append(
                $"{indentStr}*** Characteristic-Info ***:" + Environment.NewLine +
                $"{indentStr}Characteristic-UUID: {this.Uuid}" + Environment.NewLine +
                $"{indentStr}CharacteristicHandle: { this.myhandle}" + Environment.NewLine +
                $"{indentStr}I'm belonging to Service {myService.Uuid} which is connected on handle {myService.myDevice.ConnectionHandle} on Device [{BlueGigaBLEHelper.ByteArrayToHexString(myService.myDevice.DeviceAdressBytes)}] [{myService.myDevice.DeviceAdress}]" + Environment.NewLine);
            return sb.ToString();
        }
        #endregion
    }
}
