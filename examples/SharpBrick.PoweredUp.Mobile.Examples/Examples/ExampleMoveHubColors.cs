using System.Threading.Tasks;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Mobile.Examples.Examples;

namespace Example
{
    public class ExampleMoveHubColors : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using (var moveHub = Host.FindByType<MoveHub>())
            {
                await moveHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                await Task.Delay(2000);

                await moveHub.RgbLight.SetRgbColorsAsync(0xff, 0x00, 0x00);

                await Task.Delay(2000);

                await moveHub.SwitchOffAsync();
            }
        }
    }
}