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
            using (var moveHub = Host.FindByType<MoveHub>())
            {
                var device = moveHub.TiltSensor;
                await device.TiltConfigOrientationAsync(TiltConfigOrientation.Front);

                // Note that you can only have 1 notification running at a time

                await device.SetupNotificationAsync(device.ModeIndexAngle, true, 1);
                using var angleSubscription = device.AngleObservable.Subscribe(x => Log.LogWarning($"Angle: {x.x} / {x.y}"));

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                await device.SetupNotificationAsync(device.ModeIndexTilt, true, 1);
                using var tiltSubscription = device.TiltObservable.Subscribe(x => Log.LogWarning($"Tilt: {x.SI}"));

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                await device.SetupNotificationAsync(device.ModeIndexOrientation, true, 1);
                using var orientationSubscription = device.OrientationObservable.Subscribe(x => Log.LogWarning($"Orientation: {x}"));

                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                await device.SetupNotificationAsync(device.ModeIndexAcceleration, true, 1);
                using var accelerationSubscription = device.AccelerationObservable.Subscribe(x => Log.LogWarning($"Acceleration: {x.x} / {x.y} / {x.z}"));

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
}