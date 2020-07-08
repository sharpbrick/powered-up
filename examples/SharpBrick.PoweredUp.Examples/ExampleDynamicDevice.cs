using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Deployment;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace Example
{
    public class EmptyDeviceFactory : IDeviceFactory
    {
        public IPoweredUpDevice Create(DeviceType deviceType)
            => null;

        public IPoweredUpDevice CreateConnected(DeviceType deviceType, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => null;
    }
    public class ExampleDynamicDevice : BaseExample
    {
        public override void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDeviceFactory, EmptyDeviceFactory>();
        }

        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                var model = new DeploymentModelBuilder()
                    .AddAnyHub(hubBuilder => hubBuilder
                        .AddAnyDevice(0))
                    .Build();

                model.Verify(technicMediumHub.Protocol);

                var dynamicDeviceWhichIsAMotor = new DynamicDevice(technicMediumHub.Protocol, technicMediumHub.HubId, 0);
                await dynamicDeviceWhichIsAMotor.DiscoverAsync();

                logger.LogInformation("Discovery completed");

                await dynamicDeviceWhichIsAMotor.TryLockDeviceForCombinedModeNotificationSetupAsync(2, 3);
                await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(2, true);
                await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(3, true);
                await dynamicDeviceWhichIsAMotor.UnlockFromCombinedModeNotificationSetupAsync(true);

                using var disposable = dynamicDeviceWhichIsAMotor.SingleValueMode<int>(2).Observable.Subscribe(x => logger.LogWarning($"Position: {x.SI} / {x.Pct}"));
                using var disposable2 = dynamicDeviceWhichIsAMotor.SingleValueMode<short>(3).Observable.Subscribe(x => logger.LogWarning($"Absolute Position: {x.SI} / {x.Pct}"));

                await dynamicDeviceWhichIsAMotor.SingleValueMode<sbyte>(0).WriteDirectModeDataAsync(0x64); // That is StartPower on a motor

                await Task.Delay(2_000);

                await dynamicDeviceWhichIsAMotor.SingleValueMode<sbyte>(0).WriteDirectModeDataAsync(0x00); // That is Stop on a motor

                await Task.Delay(10_000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}