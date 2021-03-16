using System;
using System.Threading.Tasks;
using SharpBrick.PoweredUp;
using SharpBrick.PoweredUp.BlueGigaBLE;

namespace Example
{
    public class ExampleTwoHubsMotorControl : BaseExample
    {
        //change the Hub-adresses or use the ulong if you know them better
        public ulong BluetoothAddressHub1 = BlueGigaBLEHelper.ByteArrayToUlong(new byte[] { 0xc6, 0x43, 0x4D, 0x2b, 0x84, 0x90 });
        public ulong BluetoothAddressHub2 = BlueGigaBLEHelper.ByteArrayToUlong(new byte[] { 0xf0, 0xff, 0x4c, 0x2b, 0x84, 0x90 });

        public TechnicMediumHub DirectlyConnectedHub1 { get; private set; }
        public TechnicMediumHub DirectlyConnectedHub2 { get; private set; }

        // device needs to be switched on!
        public override async Task DiscoverAsync(bool enableTrace)
        {
            var hub1 = Host.Create<TechnicMediumHub>(BluetoothAddressHub1);
            var hub2 = Host.Create<TechnicMediumHub>(BluetoothAddressHub2);
            await hub1.ConnectAsync();
            hub1.Configure(0x00);
            await hub2.ConnectAsync();
            hub2.Configure(0x01);
            SelectedHub = DirectlyConnectedHub1 = hub1;
            DirectlyConnectedHub2 = hub2;

        }

        public override async Task ExecuteAsync()
        {
            using (TechnicMediumHub technicMediumHub1 = DirectlyConnectedHub1,
                 technicMediumHub2 = DirectlyConnectedHub2)
            {
                await technicMediumHub1.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub1.A)
                    )
                );
                await technicMediumHub2.VerifyDeploymentModelAsync(modelBuilder => modelBuilder
                    .AddHub<TechnicMediumHub>(hubBuilder => hubBuilder
                        .AddDevice<TechnicXLargeLinearMotor>(technicMediumHub2.A)
                    )
                );

                var motor1 = technicMediumHub1.A.GetDevice<TechnicXLargeLinearMotor>();
                var motor2 = technicMediumHub2.A.GetDevice<TechnicXLargeLinearMotor>();

                await motor1.SetAccelerationTimeAsync(3000);
                await motor1.SetDecelerationTimeAsync(1000);
                await motor2.SetAccelerationTimeAsync(3000);
                await motor2.SetDecelerationTimeAsync(1000);

                await motor1.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);
                await motor2.StartSpeedForTimeAsync(6000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);

                await Task.Delay(10_000);

                await motor1.StartSpeedForDegreesAsync(180, -10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await motor2.StartSpeedForDegreesAsync(180, -10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await Task.Delay(10_000);

                await motor1.StartSpeedAsync(100, 90, SpeedProfiles.None);
                await motor2.StartSpeedAsync(100, 90, SpeedProfiles.None);
                
                await Task.Delay(2000);
                
                await motor1.StartSpeedAsync(-100, 90, SpeedProfiles.None);
                await motor2.StartSpeedAsync(-100, 90, SpeedProfiles.None);
                
                await Task.Delay(2000);
                
                await motor1.StartSpeedAsync(0, 90, SpeedProfiles.None);
                await motor2.StartSpeedAsync(0, 90, SpeedProfiles.None);

                await Task.Delay(10_000);

                await motor1.GotoPositionAsync(45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await motor2.GotoPositionAsync(45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                
                await Task.Delay(2000);
                
                await motor1.GotoPositionAsync(-45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);
                await motor2.GotoPositionAsync(-45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None);

                await technicMediumHub1.SwitchOffAsync();
                await technicMediumHub2.SwitchOffAsync();
            }
        }
    }
}