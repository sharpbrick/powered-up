using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await Example.ExampleColors.ExecuteAsync();
            //await Example.ExampleMotorControl.ExecuteAsync();
            //await Example.ExampleMotorInputAbsolutePosition.ExecuteAsync();
            //await Example.ExampleMotorVirtualPort.ExecuteAsync();
            //await Example.ExampleHubActions.ExecuteAsync();
            //await Example.ExampleTechnicMediumHubAccelerometer.ExecuteAsync();
            //await Example.ExampleTechnicMediumHubGyroSensor.ExecuteAsync();
            //await Example.ExampleVoltage.ExecuteAsync();
            //await Example.ExampleTechnicMediumTemperatureSensor.ExecuteAsync();
            await Example.ExampleMotorInputCombinedMode.ExecuteAsync();
        }
    }
}
