using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Functions;

namespace Example;

public class ExampleRampUp : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();

        var stopWatch = new Stopwatch();

        var motor = technicMediumHub.A.GetDevice<TechnicLargeLinearMotor>();

        // ramp up with linear speed
        var rampUp = ServiceProvider.GetService<LinearSpeedChange>();

        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Red);

        stopWatch.Start();
        await rampUp.ExecuteAsync(motor, 20, 100, 40, 10_000);
        var redPhase = stopWatch.ElapsedMilliseconds;

        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Green);

        await Task.Delay(2_000);

        await technicMediumHub.RgbLight.SetRgbColorNoAsync(PoweredUpColor.Orange);

        // ramp down with linear speed
        var rampDown = ServiceProvider.GetService<LinearSpeedChange>();

        var beforeOrangePhase = stopWatch.ElapsedMilliseconds;
        await rampDown.ExecuteAsync(motor, 100, 0, 100, 20_000);
        var orangePhase = stopWatch.ElapsedMilliseconds - beforeOrangePhase;
        stopWatch.Stop();

        await technicMediumHub.SwitchOffAsync();

        // time delays (parameter) + 100s of BLE messages async/await ops
        Log.LogInformation($"Red Phase: {redPhase}ms; Orange Phase: {orangePhase}ms");
    }
}
