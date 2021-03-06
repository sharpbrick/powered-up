using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Bluetooth.Mock;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;
using SharpBrick.PoweredUp.Protocol.Messages;
using Xunit;

namespace SharpBrick.PoweredUp
{
    public class VoltageTest
    {
        [Fact]
        public async Task Voltage_VoltageLObservable_Receive()
        {
            // arrange
            var (protocol, mock) = CreateProtocolAndMock(SystemType.LegoTechnic_MediumHub);

            // announce voltage device in protocol
            await mock.WriteUpstreamAsync(new HubAttachedIOForAttachedDeviceMessage(0x20, DeviceType.Voltage, Version.Parse("1.0.0.0"), Version.Parse("1.0.0.0"))
            {
                HubId = 0,
            });

            //await mock.WriteUpstreamAsync("0F-00-04-20-01-14-00-00-00-00-10-00-00-00-10"); 

            var voltageDevice = new Voltage(protocol, 0, 0x20);

            short actualL = 0;
            short actualS = 0;

            using var d1 = voltageDevice.VoltageLObservable.Subscribe(x => actualL = x.SI);
            using var d2 = voltageDevice.VoltageSObservable.Subscribe(x => actualS = x.SI);

            // assert and act
            Assert.Equal(0, actualL);

            await mock.WriteUpstreamAsync("06-00-45-20-0F-00");

            Assert.Equal(35, actualL);
            Assert.Equal(35, voltageDevice.VoltageL);
            Assert.Equal(0, actualS);
            Assert.Equal(0, voltageDevice.VoltageS);

            // switch to other mode
            await voltageDevice.SetupNotificationAsync(voltageDevice.ModeIndexVoltageS, true, 1);
            // .. and device confirms it
            await mock.WriteUpstreamAsync(new PortInputFormatSingleMessage(0x20, 1, 0, true)
            {
                HubId = 0,
            });

            // act
            await mock.WriteUpstreamAsync("06-00-45-20-0F-04");

            // now other mode is updated.
            Assert.Equal(35, actualL);
            Assert.Equal(35, voltageDevice.VoltageL);
            Assert.Equal(2440, actualS);
            Assert.Equal(2440, voltageDevice.VoltageS);
        }


        internal (ILegoWirelessProtocol protocol, PoweredUpBluetoothCharacteristicMock mock) CreateProtocolAndMock(SystemType knownSystemType)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddMockBluetooth()
                .AddSingleton<BluetoothKernel>()
                .AddSingleton<ILegoWirelessProtocol, LegoWirelessProtocol>()
                .AddSingleton<IDeviceFactory, DeviceFactory>() // for protocol knowledge init

                .BuildServiceProvider();

            var poweredUpBluetoothAdapterMock = serviceProvider.GetMockBluetoothAdapter();

            var protocol = serviceProvider.GetService<ILegoWirelessProtocol>();

            protocol.ConnectAsync(knownSystemType).Wait();

            return (protocol, poweredUpBluetoothAdapterMock.MockCharacteristic);
        }
    }
}