using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example;

public class ExampleMarioAccelerometer : BaseExample
{
    public override async Task ExecuteAsync()
    {
        using var mario = Host.FindByType<MarioHub>();

        await mario.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
            .AddHub<MarioHub>(hubBuilder => { })
        );

        var d1 = mario.Accelerometer.GestureObservable.Subscribe(x => Log.LogWarning($"Gesture: {x[0]}/{x[1]}"));

        await mario.Accelerometer.SetupNotificationAsync(mario.Accelerometer.ModeIndexGesture, true);

        await Task.Delay(20_000);

        d1.Dispose();

        await mario.SwitchOffAsync();
    }
}
