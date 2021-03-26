using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public BlueGigaBLEPoweredUpBluetoothService Service { get; init; }
        /// <summary>
        /// The characteristic-handle on the connection which has to be used by the bluegiga-adapter to adress this character
        /// </summary>
        public ushort CharacteristicHandle { get; init; }
        /// <summary>
        /// The Logger for this object
        /// </summary>
        public ILogger Logger { get; init; }
        #endregion
        #region Constructors
        public BlueGigaBLEPoweredUpBluetoothCharacteristic(BlueGigaBLEPoweredUpBluetoothService service, ushort handle, Guid characteristicUuid, ILogger logger, bool traceDebug)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            CharacteristicHandle = handle;
            Uuid = characteristicUuid;
            Logger = logger;
            TraceDebug = traceDebug;
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
            //attach the notification-handler
            Service.Device.BleDevice.CharacteristicsByUuid[Uuid].ValueChanged += AttributeValueChangedEventHandler;
            void AttributeValueChangedEventHandler(object sender, BGLibExt.BleCharacteristicValueChangedEventArgs e)
            {
                if (TraceDebug)
                {
                    Logger?.LogDebug($"Data received in characteristic [{BlueGigaBLEHelper.ByteArrayToHexString(e.Value)}]");
                }

                _ = notificationHandler(e.Value);
            }
            //write to the configuration-handler of the characteristic to enable notifications
            await Service.Device.BleDevice.CharacteristicsByUuid[Uuid].WriteCccAsync(BGLibExt.BleCccValue.NotificationsEnabled);
            return true;
        }

        public async Task<bool> WriteValueAsync(byte[] data)
        {
            //waitForEvent.Wait();

            if (TraceDebug)
            {
                Logger?.LogDebug($"WriteValueAsync: data=[{BlueGigaBLEHelper.ByteArrayToHexString(data)}]");
            }
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            //there is no WriteWithResult or alike in BgLibExt; so we assume it always goes right and retrun true!
            await Service.Device.BleDevice.CharacteristicsByUuid[Uuid].WriteValueAsync(data);
            //wait here a little bit to give the Hub a chance to fire events before next Write
            //await Task.Delay(waitTime);
            //_ = waitForEvent.Release();
            
            if (TraceDebug)
            {
                Logger?.LogDebug($"WriteValueAsync: data=[{BlueGigaBLEHelper.ByteArrayToHexString(data)}] ENDED!");
            }
            return true;
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
                    // dispose managed state (managed objects)
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
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
            var sb = new StringBuilder();
            _ = await Task.Run(() =>
        {
            var indentStr = new string('\t', indent < 0 ? 0 : indent);

            _ = sb.Append(
                $"{indentStr}*** Characteristic-Info ***:" + Environment.NewLine +
                    $"{indentStr}Characteristic-UUID: {Uuid}" + Environment.NewLine +
                    $"{indentStr}CharacteristicHandle: { CharacteristicHandle}" + Environment.NewLine +
                    $"{indentStr}I'm belonging to Service {Service.Uuid} which is connected on Device [{BlueGigaBLEHelper.ByteArrayToHexString(Service.Device.DeviceAdressBytes)}] [{Service.Device.DeviceAdress}]" + Environment.NewLine);
            return sb;
        });
            return sb.ToString();
        }
        #endregion
    }
}
