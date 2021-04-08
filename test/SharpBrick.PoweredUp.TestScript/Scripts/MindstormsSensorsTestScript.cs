using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Deployment;

namespace SharpBrick.PoweredUp.TestScript
{
    public class MindstormsSensorsTestScript : ITestScript
    {
        public void DefineDeploymentModel(DeploymentModelBuilder builder)
            => builder.AddHub<TechnicMediumHub>(hubBuilder =>
            {
                hubBuilder
                    .AddDevice<TechnicDistanceSensor>(0)
                    .AddDevice<TechnicColorSensor>(1);
            });

        public async Task ExecuteScriptAsync(Hub hub, TestScriptExecutionContext context)
        {
            var distanceSensor = hub.Port(0).GetDevice<TechnicDistanceSensor>();
            var colorSensor = hub.Port(1).GetDevice<TechnicColorSensor>();

            context.Log.LogInformation("Start Testing DistanceSensor");

            await TestCase1_TechnicDistanceSensorLightsAsync(context, distanceSensor);

            await TestCase2_TechnicDistanceSensorLightsBrightnessAsync(context, distanceSensor);

            await TestCase3_TechnicDistanceSensorShortDistanceAsync(context, distanceSensor);

            await TestCase4_TechnicDistanceSensorDistanceAsync(context, distanceSensor);
        }

        private async Task TestCase1_TechnicDistanceSensorLightsAsync(TestScriptExecutionContext context, TechnicDistanceSensor distanceSensor)
        {
            await distanceSensor.SetEyeLightAsync(leftTop: 100);
            await context.ConfirmAsync("TechnicDistanceSensor.SetEyeLightAsync: Light Left Top?");

            await distanceSensor.SetEyeLightAsync(rightTop: 100);
            await context.ConfirmAsync("TechnicDistanceSensor.SetEyeLightAsync: Light Right Top?");

            await distanceSensor.SetEyeLightAsync(leftBottom: 100);
            await context.ConfirmAsync("TechnicDistanceSensor.SetEyeLightAsync: Light Left Bottom?");

            await distanceSensor.SetEyeLightAsync(rightBottom: 100);
            await context.ConfirmAsync("TechnicDistanceSensor.SetEyeLightAsync: Light Right Bottom?");
        }

        private async Task TestCase2_TechnicDistanceSensorLightsBrightnessAsync(TestScriptExecutionContext context, TechnicDistanceSensor distanceSensor)
        {
            // play with the eye lights (0-100)
            for (byte idx = 0; idx < 100; idx += 5)
            {
                await distanceSensor.SetEyeLightAsync(0b0000_0000, idx, 0b0110_0100, idx);

                await Task.Delay(500);
            }

            await context.ConfirmAsync("TechnicDistanceSensor.SetEyeLightAsync: Did the right side brighten up?");
        }

        private async Task TestCase3_TechnicDistanceSensorShortDistanceAsync(TestScriptExecutionContext context, TechnicDistanceSensor distanceSensor)
        {
            context.Log.LogInformation("Test 0 at more than 32cm; Test distances between 4 and 32cm for 10s");
            using var d1 = distanceSensor.ShortOnlyDistanceObservable.Subscribe(x => context.Log.LogInformation($"Distance (short): {x}"));

            await distanceSensor.SetupNotificationAsync(distanceSensor.ModeIndexShortOnlyDistance, true);

            await Task.Delay(10_000);

            await distanceSensor.SetupNotificationAsync(distanceSensor.ModeIndexShortOnlyDistance, false);

            await context.ConfirmAsync("Are the distances measured accurately?");
        }

        private async Task TestCase4_TechnicDistanceSensorDistanceAsync(TestScriptExecutionContext context, TechnicDistanceSensor distanceSensor)
        {
            context.Log.LogInformation("Test distances between 4 and 250cm for 10s");
            using var d1 = distanceSensor.DistanceObservable.Subscribe(x => context.Log.LogInformation($"Distance: {x}"));

            await distanceSensor.SetupNotificationAsync(distanceSensor.ModeIndexDistance, true);

            await Task.Delay(10_000);

            await distanceSensor.SetupNotificationAsync(distanceSensor.ModeIndexDistance, false);

            await context.ConfirmAsync("Are the distances measured accurately?");
        }
    }
}