using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;

namespace SharpBrick.PoweredUp
{
    public class PoweredUpBluetoothAdapterMock : IPoweredUpBluetoothAdapter
    {
        public PoweredUpBluetoothAdapterMock()
        {
            MockCharacteristic = new PoweredUpBluetoothCharacteristicMock();
            MockService = new PoweredUpBluetoothServiceMock(MockCharacteristic);
            MockDevice = new PoweredUpBluetoothDeviceMock(MockService);
        }

        public PoweredUpBluetoothDeviceMock MockDevice { get; }
        public PoweredUpBluetoothServiceMock MockService { get; }
        public PoweredUpBluetoothCharacteristicMock MockCharacteristic { get; }

        public void Discover(Func<PoweredUpBluetoothDeviceInfo,Task> discoveryHandler, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPoweredUpBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress)
        {
            return Task.FromResult<IPoweredUpBluetoothDevice>(MockDevice);
        }
    }

    public class PoweredUpBluetoothDeviceMock : IPoweredUpBluetoothDevice
    {
        private PoweredUpBluetoothServiceMock _mockService;

        public PoweredUpBluetoothDeviceMock(PoweredUpBluetoothServiceMock mockService)
        {
            this._mockService = mockService;
        }

        public string Name => "Mock Device";

        public void Dispose()
        { }

        public Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
            => Task.FromResult<IPoweredUpBluetoothService>(_mockService);
    }

    public class PoweredUpBluetoothServiceMock : IPoweredUpBluetoothService
    {
        private PoweredUpBluetoothCharacteristicMock _mockCharacteristic;

        public PoweredUpBluetoothServiceMock(PoweredUpBluetoothCharacteristicMock mockCharacteristic)
        {
            this._mockCharacteristic = mockCharacteristic;
        }

        public Guid Uuid => Guid.Empty;

        public void Dispose()
        {
        }

        public Task<IPoweredUpBluetoothCharacteristic> GetCharacteristicAsync(Guid guid)
            => Task.FromResult<IPoweredUpBluetoothCharacteristic>(_mockCharacteristic);
    }

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
            => WriteUpstreamAsync(MessageEncoder.Encode(new HubAttachedIOForAttachedDeviceMessage()
            {
                HubId = hubId,
                PortId = portId,
                IOTypeId = deviceType,
                HardwareRevision = hw,
                SoftwareRevision = sw,
            }, null));
    }
}