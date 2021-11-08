using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleSetHubProperty : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var technicMediumHub = Host.FindByType<TechnicMediumHub>();
        using var d1 = technicMediumHub.AdvertisementNameObservable.Subscribe(x => Log.LogInformation($"From Observable for AdvertisementName: {x}"));
        using var d2 = technicMediumHub.PropertyChangedObservable.Subscribe(x => Log.LogInformation($"Property Changed: {x}"));

        var originalName = technicMediumHub.AdvertisingName;

        Log.LogInformation($"Original Name: {originalName}");

        await technicMediumHub.SetAdvertisingNameAsync("Hello World");

        Log.LogInformation($"New Name is: {technicMediumHub.AdvertisingName}");

        await technicMediumHub.ResetAdvertisingNameAsync();

        Log.LogInformation($"Resetted Name is: {technicMediumHub.AdvertisingName}");

        await technicMediumHub.SetAdvertisingNameAsync(originalName);

        Log.LogInformation($"Name Cleanup to: {originalName}");

        await technicMediumHub.SwitchOffAsync();
    }
}
