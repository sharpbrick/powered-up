﻿using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using SharpBrick.PoweredUp.Bluetooth;

namespace SharpBrick.PoweredUp.BlueGigaBLE;

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
    /// <summary>
    /// Enables the notification on this characteristic and attaches the notificationHandler to the fired notifications
    /// </summary>
    /// <param name="notificationHandler">The handler that shall be called when a notification from the characteristic is fired</param>
    /// <returns>Always true, because we donÄt get back a result of the write-operation; assume it as successful</returns>
    /// <exception cref="ArgumentNullException">Thrown when th handler is null</exception>
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
        //BGLibExt does not have a WriteWithResult or alike; so we assume the write was successful
        return true;
    }
    /// <summary>
    /// Writes the data to the characteristic
    /// </summary>
    /// <param name="data">The data that shall be written to the characteristic</param>
    /// <returns>Always true, because we donÄt get back a result of the write-operation; assume it as successful</returns>
    /// <exception cref="ArgumentNullException">Thrown when the data is null</exception>
    public async Task<bool> WriteValueAsync(byte[] data)
    {
        if (TraceDebug)
        {
            Logger?.LogDebug($"WriteValueAsync: data=[{BlueGigaBLEHelper.ByteArrayToHexString(data)}]");
        }
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        await Service.Device.BleDevice.CharacteristicsByUuid[Uuid].WriteValueAsync(data);
        if (TraceDebug)
        {
            Logger?.LogDebug($"WriteValueAsync: data=[{BlueGigaBLEHelper.ByteArrayToHexString(data)}] ENDED!");
        }
        //there is no WriteWithResult or alike in BgLibExt; so we assume it always goes right and retrun true!
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
