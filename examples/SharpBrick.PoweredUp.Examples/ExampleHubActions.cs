using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleHubActions : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0xff);

        await Task.Delay(2000);

        await technicMediumHub.ActivateBusyIndicatorAsync(); // technic medium hub => blinking with color from above

        await Task.Delay(5000);

        await technicMediumHub.ResetBusyIndicatorAsync(); // stop blinking

        await Task.Delay(2000);

        await technicMediumHub.SwitchOffAsync();
    }
}
