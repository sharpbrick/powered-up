using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Bluetooth.Mock;

public class PoweredUpBluetoothDeviceMock : IPoweredUpBluetoothDevice
{
    private readonly PoweredUpBluetoothServiceMock _mockService;

    public PoweredUpBluetoothDeviceMock(PoweredUpBluetoothServiceMock mockService)
    {
        this._mockService = mockService;
    }

    public string Name => "Mock Device";

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task<IPoweredUpBluetoothService> GetServiceAsync(Guid serviceId)
        => Task.FromResult<IPoweredUpBluetoothService>(_mockService);
}
