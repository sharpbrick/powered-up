using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpBrick.PoweredUp.Bluetooth;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Protocol.Parsers;
using SharpBrick.PoweredUp.WinRT;

namespace SharpBrick.PoweredUp.Examples.MessageTrace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var poweredUpBluetoothAdapter = new WinRTPoweredUpBluetoothAdapter();

            Console.WriteLine("Finding Service");
            ulong bluetoothAddress = 0;
            var cts = new CancellationTokenSource();
            poweredUpBluetoothAdapter.Discover(info =>
            {
                Console.WriteLine($"Found {info.BluetoothAddress}");

                bluetoothAddress = info.BluetoothAddress;
            }, cts.Token);

            Console.WriteLine("Press any key to cancel Scanning");
            Console.ReadLine();

            cts.Cancel();

            using (var device = await poweredUpBluetoothAdapter.GetDeviceAsync(bluetoothAddress))
            {
                Console.WriteLine($"Device Name: {device.Name}");

                using (var service = await device.GetServiceAsync(new Guid(PoweredUpBluetoothConstants.LegoHubService)))
                {
                    Console.WriteLine($"GATT Service {service.Uuid}");

                    var characteristic = await service.GetCharacteristicAsync(new Guid(PoweredUpBluetoothConstants.LegoHubCharacteristic));
                    Console.WriteLine($"GATT Characteristics {characteristic.Uuid}");

                    await characteristic.NotifyValueChangeAsync(async data =>
                    {
                        var message = MessageParser.Decode(data);

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
                            UnknownMessage msg => $"Unknown Message Type: {(MessageType)msg.MessageType} Length: {msg.Length} Content: {DataToString(data)}",
                            var foo => $"{foo.MessageType} (not yet formatted)",
                        };

                        Console.WriteLine(messageAsString);
                        Console.WriteLine(DataToString(data));
                    });

                    byte[] request = MessageParser.Encode(new HubPropertyMessage() { Property = HubProperty.AdvertisingName, Operation = HubPropertyOperation.RequestUpdate });
                    Console.WriteLine($"Request: {DataToString(request)}");
                    await characteristic.WriteValueAsync(request);

                    Console.ReadLine();
                }
            }
        }

        public static string DataToString(byte[] data)
            => string.Join("-", data.Select(b => $"{b:X2}"));
    }
}
