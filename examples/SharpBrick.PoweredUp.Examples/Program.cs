using System;
using System.Threading.Tasks;

namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var enableTrace = (args.Length > 0 && args[0] == "--trace");

            // NOTE: Examples are in their own root namespace to make namespace usage clear
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
            //example = new Example.ExampleDynamicDevice();
            //example = new Example.ExampleBluetoothByKnownAddress();
            //example = new Example.ExampleBluetoothByName();
            //example = new Example.ExampleSetHubProperty();
            //example = new Example.ExampleHubPropertyObserving();
            //example = new Example.ExampleDiscoverByType();
            //example = new Example.ExampleCalibrationSteering();
            example = new Example.ExampleTechnicMediumHubGestSensor();

            // example = new Example.ExampleMarioBarcode();
            // example = new Example.ExampleMarioPants();

            // NOTE: Examples are programmed object oriented style. Base class implements methods Configure, DiscoverAsync and ExecuteAsync to be overwriten on demand.
            await example.InitHostAndDiscoverAsync(enableTrace);

            if (example.SelectedHub != null)
            {
                await example.ExecuteAsync();
            }
        }
    }
}
