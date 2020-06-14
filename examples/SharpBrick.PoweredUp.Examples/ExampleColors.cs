using System.Threading.Tasks;
using SharpBrick.PoweredUp;

namespace Example
{
    public static class ExampleColors
    {
        public static async Task ExecuteAsync()
        {
            var (host, serviceProvider, _) = ExampleHubDiscover.CreateHostAndDiscover();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                await technicMediumHub.RgbLight.SetRgbColorsAsync(0x00, 0xff, 0x00);

                await Task.Delay(2000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}