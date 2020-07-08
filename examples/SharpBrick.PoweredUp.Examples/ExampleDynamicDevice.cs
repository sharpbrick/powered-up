using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol;

namespace Example
{
    public class EmptyDeviceFactory : IDeviceFactory
    {
        public IPoweredUpDevice Create(DeviceType deviceType)
            => null;

        public IPoweredUpDevice CreateConnected(DeviceType deviceType, IPoweredUpProtocol protocol, byte hubId, byte portId)
            => new DynamicDevice(protocol, hubId, portId);
    }
    public class ExampleDynamicDevice : BaseExample
    {
        public override void Configure(IServiceCollection services)
        {
            // pretend we do not know the device and use only dynamic ones
            services.AddSingleton<IDeviceFactory, EmptyDeviceFactory>();
        }

        public override async Task ExecuteAsync()
        {
            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ExampleMotorInputAbsolutePosition>();

            using (var technicMediumHub = host.FindByType<TechnicMediumHub>())
            {
                // deployment model verification with unknown devices
                await technicMediumHub.VerifyDeploymentModelAsync(mb => mb
                    .AddAnyHub(hubBuilder => hubBuilder
                        .AddAnyDevice(0))
                    );

                var dynamicDeviceWhichIsAMotor = technicMediumHub.Port(0).GetDevice<DynamicDevice>();

                // or also direct from a protocol
                //var dynamicDeviceWhichIsAMotor = new DynamicDevice(technicMediumHub.Protocol, technicMediumHub.HubId, 0);

                // discover the unknown device using the LWP
                await dynamicDeviceWhichIsAMotor.DiscoverAsync();
                logger.LogInformation("Discovery completed");

                // use combined mode values from the device
                await dynamicDeviceWhichIsAMotor.TryLockDeviceForCombinedModeNotificationSetupAsync(2, 3);
                await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(2, true);
                await dynamicDeviceWhichIsAMotor.SetupNotificationAsync(3, true);
                await dynamicDeviceWhichIsAMotor.UnlockFromCombinedModeNotificationSetupAsync(true);

                // get the individual modes for input and output
                var powerMode = dynamicDeviceWhichIsAMotor.SingleValueMode<sbyte>(0);
                var posMode = dynamicDeviceWhichIsAMotor.SingleValueMode<int>(2);
                var aposMode = dynamicDeviceWhichIsAMotor.SingleValueMode<short>(3);

                // use their observables to report values
                using var disposable = posMode.Observable.Subscribe(x => logger.LogWarning($"Position: {x.SI} / {x.Pct}"));
                using var disposable2 = aposMode.Observable.Subscribe(x => logger.LogWarning($"Absolute Position: {x.SI} / {x.Pct}"));

                // or even write to them
                await powerMode.WriteDirectModeDataAsync(0x64); // That is StartPower on a motor
                await Task.Delay(2_000);
                await powerMode.WriteDirectModeDataAsync(0x00); // That is Stop on a motor

                await Task.Delay(10_000);

                await technicMediumHub.SwitchOffAsync();
            }
        }
    }
}