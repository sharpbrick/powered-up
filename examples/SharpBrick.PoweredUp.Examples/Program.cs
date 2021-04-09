using System;
using System.Linq;
using System.Threading.Tasks;


namespace SharpBrick.PoweredUp.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var enableTrace = (args.Length > 0 && args[0] == "--trace");
            string bluetoothStackPort = "WINRT";
            bool enableTraceBlueGiga = false;
            if (args.Any(x => x.Equals("--usebluegiga", StringComparison.OrdinalIgnoreCase)))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Equals("--usebluegiga", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length > i)
                        {
                            bluetoothStackPort = args[i + 1];
                        }
                        break;
                    }
                }
            }
            if (args.Any(x => x.Equals("--tracebluegiga", StringComparison.OrdinalIgnoreCase)))
            {
                enableTraceBlueGiga = true;
            }

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
            //example = new Example.ExampleRampUp();
            //example = new Example.ExampleTechnicMediumHubGestSensor();
            //example = new Example.ExampleRemoteControlButton();
            //example = new Example.ExampleRemoteControlRssi();
            //example = new Example.ExampleTechnicMediumAngularMotorGrey();
            //example = new Example.ExampleMarioBarcode();
            //example = new Example.ExampleMarioPants();
            //example = new Example.ExampleMarioAccelerometer();
            //example = new Example.ExampleDuploTrainBase();
            //example = new Example.ExampleTechnicColorSensor();
            //example = new Example.ExampleTechnicDistanceSensor();
            //example = new Example.ExampleTechnicMediumHubGestSensor();
            //example = new Example.ExampleMoveHubInternalTachoMotorControl();
            //example = new Example.ExampleMoveHubExternalMediumLinearMotorControl();
            //example = new Example.ExampleMoveHubColors();
            //example = new Example.ExampleMoveHubTiltSensor();
            //example = new ExampleTwoHubsMotorControl();
            //example = new ExampleTwoPortHubMediumLinearMotor();
            example = new Example.ExampleColorDistanceSensor();

            // NOTE: Examples are programmed object oriented style. Base class implements methods Configure, DiscoverAsync and ExecuteAsync to be overwriten on demand.
            // this uses the WinRT-bluetooth-implementation by default
            await example.InitHostAndDiscoverAsync(enableTrace);
            //for using BlueGiga-Bluetoothadapter:
            //await example.InitHostAndDiscoverAsync(enableTrace, bluetoothStackPort, enableTraceBlueGiga);

            if (example.SelectedHub is not null)
            {
                await example.ExecuteAsync();
            }
        }
    }
}
