using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleBluetoothByKnownAddress : BaseExample
    {
        public const ulong ChangeMe_BluetoothAddress = 158897336311065;
        public TechnicMediumHub DirectlyConnectedHub { get; private set; }

        // device needs to be switched on!
        public override void Discover(bool enableTrace)
        {
            var hub = host.Create<TechnicMediumHub>(ChangeMe_BluetoothAddress);

            selectedHub = DirectlyConnectedHub = hub;

            hub.ConnectAsync().Wait();
        }

        public override async Task ExecuteAsync()
        {
            using (var technicMediumHub = DirectlyConnectedHub)
            {
                await technicMediumHub.RgbLight.SetRgbColorsAsync(0xff, 0x00, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0xff, 0xff, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}