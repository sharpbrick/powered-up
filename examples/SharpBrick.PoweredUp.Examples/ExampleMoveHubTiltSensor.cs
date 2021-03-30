using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp;

namespace Example
{
    public class ExampleMoveHubTiltSensor : BaseExample
    {
        public override async Task ExecuteAsync()
        {
            using var moveHub = Host.FindByType<MoveHub>();

            var device = moveHub.TiltSensor;
            await device.TiltConfigOrientationAsync(TiltConfigOrientation.Front);

            // Note that you can only have 1 notification running at a time

            await device.SetupNotificationAsync(device.ModeIndexTwoAxisFull, true, 1);
            using var twoAxisFullSubscription = device.TwoAxisFullObservable.Subscribe(x => Log.LogWarning($"Two Axis Values - Roll: {x.roll}, Pitch: {x.pitch}"));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            await device.SetupNotificationAsync(device.ModeIndexTwoAxisState, true, 1);
            using var twoAxisStateSubscription = device.TwoAxisStateObservable.Subscribe(x => Log.LogWarning($"Two Axis State: {x}"));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            await device.SetupNotificationAsync(device.ModeIndexThreeAxisState, true, 1);
            using var threeAxisStateSubscription = device.ThreeAxisStateObservable.Subscribe(x => Log.LogWarning($"Three Axis State: {x}"));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            await device.SetupNotificationAsync(device.ModeIndexThreeAxisFull, true, 1);
            using var threeAxisFullSubscription = device.ThreeAxisFullObservable.Subscribe(x => Log.LogWarning($"Three Axis Values - Roll: {x.roll}, Pitch: {x.pitch}, Yaw: {x.yaw}"));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            // This configures a minimum threshold for an impact to be registered (should be a light tap) and subscribes to the count of impacts
            await device.TiltConfigImpactAsync(10, 1270);
            await device.SetupNotificationAsync(device.ModeIndexImpacts, true, deltaInterval: 1);
            using var impactSubscription = device.ImpactsObservable.Subscribe(x => Log.LogWarning($"Impact Count: {x.SI}"));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            await moveHub.SwitchOffAsync();
        }
    }
}