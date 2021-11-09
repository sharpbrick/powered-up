using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp.Bluetooth.Mock;

public class PoweredUpBluetoothCharacteristicMock : IPoweredUpBluetoothCharacteristic
{
    private Func<byte[], Task> _next;

    public Guid Uuid => Guid.Empty;

    public Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler)
    {
        _next = notificationHandler;

        return Task.FromResult(true);
    }

    public Task<bool> WriteValueAsync(byte[] data)
    {
        DownstreamMessages.Add(data);

        return Task.FromResult(true);
    }

    public Task WriteUpstreamAsync(LegoWirelessMessage message)
        => WriteUpstreamAsync(MessageEncoder.Encode(message, null));

    public Task WriteUpstreamAsync(string message)
        => WriteUpstreamAsync(BytesStringUtil.StringToData(message));

    public Task WriteUpstreamAsync(params byte[] message)
        => _next(message);

    public List<byte[]> DownstreamMessages { get; } = new List<byte[]>();

    public Task AttachIO(DeviceType deviceType, byte hubId, byte portId, Version hw, Version sw)
        => WriteUpstreamAsync(MessageEncoder.Encode(new HubAttachedIOForAttachedDeviceMessage(portId, deviceType, hw, sw)
        {
            HubId = hubId,
        }, null));
}
