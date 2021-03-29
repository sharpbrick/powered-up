using System.Threading.Tasks;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Mobile.Examples.Examples;

namespace Example
{
    public class ExampleBluetoothByKnownAddress : BaseExample
    {
        public const ulong ChangeMe_BluetoothAddress = 158897336311065;
        public TechnicMediumHub DirectlyConnectedHub { get; private set; }

        // device needs to be switched on!
        public override async Task DiscoverAsync(bool enableTrace)
        {
            var hub = Host.Create<TechnicMediumHub>(ChangeMe_BluetoothAddress);

            SelectedHub = DirectlyConnectedHub = hub;

            await hub.ConnectAsync();
        }

        public override async Task ExecuteAsync()
        {
            using (var technicMediumHub = DirectlyConnectedHub)
            {
                await technicMediumHub.RgbLight.SetRgbColorsAsync(0xff, 0x00, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0xff, 0x00, 0xff);

                await Task.Delay(2000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}