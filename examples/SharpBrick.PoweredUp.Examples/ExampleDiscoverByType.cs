using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleDiscoverByType : BaseExample
    {
        public TechnicMediumHub DirectlyConnectedHub { get; private set; }

        // device needs to be switched on!
        public override async Task DiscoverAsync(bool enableTrace)
        {
            var hub = await Host.DiscoverAsync<TechnicMediumHub>();

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

                await technicMediumHub.RgbLight.SetRgbColorsAsync(0xff, 0xff, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}