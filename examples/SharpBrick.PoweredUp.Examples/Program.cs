using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var enableTrace = (args.Length > 0 && args[0] == "--trace");

            var (host, serviceProvider, selectedHub) = Example.ExampleHubDiscover.CreateHostAndDiscover(enableTrace);

            //await Example.ExampleColors.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleMotorControl.ExecuteAsynchost, serviceProvider, selectedHub
            //await Example.ExampleMotorInputAbsolutePosition.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleMotorVirtualPort.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleHubActions.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleTechnicMediumHubAccelerometer.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleTechnicMediumHubGyroSensor.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleVoltage.ExecuteAsync(host, serviceProvider, selectedHub);
            //await Example.ExampleTechnicMediumTemperatureSensor.ExecuteAsync(host, serviceProvider, selectedHub);
            await Example.ExampleMotorInputCombinedMode.ExecuteAsync(host, serviceProvider, selectedHub);
        }
    }
}
