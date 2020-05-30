using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Protocol.Formatter;
using SharpBrick.PoweredUp.WinRT;
using Microsoft.Extensions.Logging;

namespace SharpBrick.PoweredUp.Examples.MessageTrace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddConsole()
                    .AddFilter("SharpBrick.PoweredUp.Bluetooth.BluetoothKernel", LogLevel.Debug)
            );

            var logger = loggerFactory.CreateLogger("Main");

            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            logger.LogInformation("Finding Service");
            ulong bluetoothAddress = 0;
            var cts = new CancellationTokenSource();
            poweredUpBluetoothAdapter.Discover(info =>
            {
                logger.LogInformation($"Found {info.BluetoothAddress} is a {(PoweredUpManufacturerDataConstants)info.ManufacturerData[1]}");

                bluetoothAddress = info.BluetoothAddress;
            }, cts.Token);

            logger.LogInformation("Press any key to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();

            // ulong bluetoothAddress = 158897336311065;

            using (var kernel = new BluetoothKernel(poweredUpBluetoothAdapter, bluetoothAddress, loggerFactory.CreateLogger<BluetoothKernel>()))
            {
                await kernel.ConnectAsync();

                await kernel.ReceiveBytesAsync(async data =>
                {
                    var message = MessageEncoder.Decode(data);

                    string messageAsString = message switch
                    {
                        HubAttachedIOForAttachedDeviceMessage msg => $"Attached IO at Port {msg.PortId} of type {msg.IOTypeId} (HW: {msg.HardwareRevision} / SW: {msg.SoftwareRevision})",
                        HubAttachedIOForDetachedDeviceMessage msg => $"Dettached IO at Port {msg.PortId}",
                        HubPropertyMessage<Version> msg => $"Hub Property {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<string> msg => $"Hub Property {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<bool> msg => $"Hub Property {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<sbyte> msg => $"Hub Property {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<byte> msg => $"Hub Property {msg.Property}: {msg.Payload}",
                        HubPropertyMessage<byte[]> msg => $"Hub Property {msg.Property}: {DataToString(msg.Payload)}",
                        HubActionMessage msg => $"Hub Action {msg.Action}",
                        ErrorMessage msg => $"Error {msg.ErrorCode} from {(MessageType)msg.CommandType}",
                        UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {DataToString(data)}",
                        var foo => $"{foo.MessageType} (not yet formatted)",
                    };

                    logger.LogInformation(messageAsString);
                });

                byte[] request = MessageEncoder.Encode(new HubPropertyMessage() { Property = HubProperty.BatteryVoltage, Operation = HubPropertyOperation.RequestUpdate });
                await kernel.SendBytesAsync(request);

                Console.ReadLine();

                logger.LogInformation("Switch off device");
                request = MessageEncoder.Encode(new HubActionMessage() { Action = HubAction.SwitchOffHub });
                await kernel.SendBytesAsync(request);

                Console.ReadLine();
            }
        }

        public static string DataToString(byte[] data)
                => string.Join("-", data.Select(b => $"{b:X2}"));
    }
}
