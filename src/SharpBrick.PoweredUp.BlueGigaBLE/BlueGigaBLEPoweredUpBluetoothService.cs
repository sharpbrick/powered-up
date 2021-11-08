using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

using BGLibExt;

using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.BlueGigaBLE;

public class BlueGigaBLEPoweredUpBluetoothService : IPoweredUpBluetoothService, IBlueGigaLogger
{
    #region Properties
    /// <summary>
    /// The device (LEGO-Hub) on which this service is available
    /// </summary>
    public BlueGigaBLEPoweredUpBluetoothDevice Device { get; }
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
    /// Get a Characteristic by its Characteristic-UUID
    /// </summary>
    /// <param name="guid">The UUID of the wished characteristic</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when the wished characteristic does not exist in the GATT-service</exception>
    public async Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
    {
        BleCharacteristic characteristic;
        try
        {
            characteristic = Device.BleDevice.CharacteristicsByUuid[guid];
        }
        catch
        {
            throw new ArgumentException($"The service with GUID {Uuid} does not know a characteristic with GUID {guid} ");
        }
        var gattCharacteristic = new BlueGigaBLEPoweredUpBluetoothCharacteristic(this, characteristic.Handle, characteristic.Uuid, Logger, TraceDebug);
        return await Task.FromResult(GATTCharacteristics.AddOrUpdate(guid, gattCharacteristic, (oldkey, oldvalue) => oldvalue = gattCharacteristic));
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
        var indentStr = new string('\t', indent < 0 ? 0 : indent);
        var sb = new StringBuilder();
        _ = await Task.Run(async () =>
        {
            _ = sb.Append($"{indentStr}*** Service-Info ***:" + Environment.NewLine +
                    $"{indentStr}Service-UUID: {Uuid}" + Environment.NewLine +
                    $"{indentStr}First Characteristic-Handle: { FirstCharacterHandle}" + Environment.NewLine +
                    $"{indentStr}Last Characteristic-Handle: { LastCharacterHandle}" + Environment.NewLine +
                    $"{indentStr}I'm connected on Device [{BlueGigaBLEHelper.ByteArrayToHexString(Device.DeviceAdressBytes)}] [{Device.DeviceAdress}]" + Environment.NewLine);
            if (GATTCharacteristics?.Count > 0)
            {
                _ = sb.Append($"{indentStr}I know about the following {GATTCharacteristics.Count} characteristics from explicit use:{Environment.NewLine}");
                foreach (var characteristic in GATTCharacteristics)
                {
                    _ = sb.Append(await characteristic.Value.GetLogInfosAsync(indent + 1));
                }

                _ = sb.Append($"{indentStr}End of my known characteristics{Environment.NewLine}");
            }
            else
            {
                _ = sb.Append($"{indentStr}I DON'T know about any characteristics I had in use!{Environment.NewLine}");
            }
            return sb;
        });
        return sb.ToString();

    }
    #endregion
}
