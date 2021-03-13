using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleBluetoothByName : BaseExample
    {
        public TechnicMediumHub DirectlyConnectedHub { get; private set; }

        public override async Task ExecuteAsync()
        {
            using var technicMediumHub = Host.FindByName<TechnicMediumHub>("Technic Hub");

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