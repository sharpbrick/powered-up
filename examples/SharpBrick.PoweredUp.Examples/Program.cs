using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var enableTrace = (args.Length > 0 && args[0] == "--trace");

            //await ; return;

            Example.BaseExample example;

            //example = new Example.ExampleColors();
            //example = new Example.ExampleMotorControl();
            //example = new Example.ExampleMotorInputAbsolutePosition();
            //example = new Example.ExampleMotorVirtualPort();
            //example = new Example.ExampleHubActions();
            //example = new Example.ExampleTechnicMediumHubAccelerometer();
            //example = new Example.ExampleTechnicMediumHubGyroSensor();
            //example = new Example.ExampleVoltage();
            //example = new Example.ExampleTechnicMediumTemperatureSensor();
            //example = new Example.ExampleMotorInputCombinedMode();
            //example = new Example.ExampleMixedBag();
            //example = new Example.ExampleHubAlert();
            //example = new Example.ExampleTechnicMediumHubTiltSensor();
            //example = new Example.ExampleTechnicMediumHubTiltSensorImpacts();
            example = new Example.ExampleDynamicDevice();

            example.CreateHostAndDiscover(enableTrace);

            if (example.selectedHub != null)
            {
                await example.ExecuteAsync();
            }
        }
    }
}
