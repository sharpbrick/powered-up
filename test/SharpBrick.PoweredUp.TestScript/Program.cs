using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Deployment;
using SharpBrick.PoweredUp.Hubs;

namespace SharpBrick.PoweredUp.TestScript
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("poweredup.json", true)
                .AddCommandLine(args)
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                })
                .AddPoweredUp();

            var bluetoothAdapter = configuration["BluetoothAdapter"] ?? "WinRT";

#if WINDOWS
            if (bluetoothAdapter == "WinRT")
            {
                serviceCollection.AddWinRTBluetooth();
            }
#endif

#if NET5_0_OR_GREATER
            if (bluetoothAdapter == "BlueGigaBLE")
            {
                // config for "COMPortName" and "TraceDebug" (either via command line or poweredup.json)
                serviceCollection.AddBlueGigaBLEBluetooth(configuration);
            }
#endif

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var host = serviceProvider.GetService<PoweredUpHost>();

            IEnumerable<ITestScript> scripts = new ITestScript[] {
                new TechnicMotorTestScript<TechnicLargeLinearMotor>(),
                new MindstormsSensorsTestScript(),
            };

            var context = new TestScriptExecutionContext(serviceProvider.GetService<ILogger<TestScriptExecutionContext>>());

            foreach (var script in scripts)
            {
                // Test Script
                context.Log.LogInformation($"Execute Script {script.GetType().Name}");

                // build deployment model
                var model = BbuildDeploymentModel(script);
                PrintModel(context, model);

                // Accept to execute Test Script
                var executeTest = await context.ConfirmAsync("> Confirm to execute Test Script");

                if (executeTest)
                {
                    context.Log.LogInformation("> Discovering & Connecting Hub");
                    var hubType = HubFactory.GetTypeFromSystemType(model.Hubs[0].HubType ?? throw new InvalidOperationException("Specify the hub type in the test script."));
                    using var hub = await host.DiscoverAsync(hubType);
                    await hub.ConnectAsync();

                    context.Log.LogInformation("> Verifying Deployment Model (fix it if necessary)");
                    await hub.VerifyDeploymentModelAsync(model);

                    context.Log.LogInformation("> Start Test Script");
                    await script.ExecuteScriptAsync(hub, context);

                    context.Log.LogInformation("> Switch Off Hub");
                    await hub.SwitchOffAsync();
                }
                else
                {
                    context.Log.LogWarning($"> User decided not to execute Test Script");
                }
            }

        }

        private static void PrintModel(TestScriptExecutionContext context, DeploymentModel model)
        {
            context.Log.LogInformation($"> Deployment Model of Test Script");

            foreach (var hub in model.Hubs)
            {
                context.Log.LogInformation($" > Hub: {hub.HubType}");

                foreach (var device in hub.Devices)
                {
                    context.Log.LogInformation($"  > Device: {device.DeviceType} @ {device.PortId}");
                }
            }
        }

        private static DeploymentModel BbuildDeploymentModel(ITestScript script)
        {
            var builder = new DeploymentModelBuilder();
            script.DefineDeploymentModel(builder);
            var model = builder.Build();
            return model;
        }
    }
}
