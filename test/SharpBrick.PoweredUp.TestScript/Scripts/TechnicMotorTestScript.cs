using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpBrick.PoweredUp.Deployment;

namespace SharpBrick.PoweredUp.TestScript
{
    public class TechnicMotorTestScript<TTechnicMotor> : ITestScript where TTechnicMotor : AbsoluteMotor, IPoweredUpDevice
    {
        public void DefineDeploymentModel(DeploymentModelBuilder builder)
            => builder.AddHub<TechnicMediumHub>(hubBuilder =>
            {
                hubBuilder.AddDevice<TTechnicMotor>(0);
            });

        public async Task ExecuteScriptAsync(Hub hub, TestScriptExecutionContext context)
        {
            var motor = hub.Port(0).GetDevice<TTechnicMotor>();

            await motor.GotoRealZeroAsync();
            await Task.Delay(2000);
            await motor.SetZeroAsync();

            // TestCase: AbsoluteMotorReportsAbsolutePosition
            await motor.TryLockDeviceForCombinedModeNotificationSetupAsync(motor.ModeIndexAbsolutePosition, motor.ModeIndexPosition);
            await motor.SetupNotificationAsync(motor.ModeIndexPosition, true, 2);
            await motor.SetupNotificationAsync(motor.ModeIndexAbsolutePosition, true, 2);
            await motor.UnlockFromCombinedModeNotificationSetupAsync(true);

            await context.ConfirmAsync("AbsoluteMotor.GotoRealZeroAsync: Is in zero position? Adjust Beam to 0°?");

            await TestCase1_TachoMotorPositionByDegreesAsync(context, motor);

            await TestCase2_TachoMotorExplicitPositionAsync(context, motor);

            await TestCase3_TachoMotorHighSpeedAndFloatingAsync(context, motor);

            await TestCase4_TachoMotorPositiveNegativeSpeedForTimeAsync(context, motor);

            await TestCase5_TachoMotorAccelerationAsync(context, motor);

            await TestCase6_BasicMotorAsync(context, motor);

            await TestCase7_InputAsync(context, motor);
        }

        private static async Task TestCase1_TachoMotorPositionByDegreesAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: Testing positioning by degress");

            await motor.StartSpeedForDegreesAsync(45, 10, 100);
            await Task.Delay(2000);
            context.Assert(motor.AbsolutePosition, 42, 48);
            context.Assert(motor.Position, 42, 48);

            await motor.StartSpeedForDegreesAsync(45, 10, 100);
            await Task.Delay(2000);
            context.Assert(motor.AbsolutePosition, 87, 93);
            context.Assert(motor.Position, 87, 93);

            await motor.StartSpeedForDegreesAsync(45, 10, 100);
            await Task.Delay(2000);
            context.Assert(motor.AbsolutePosition, 132, 138);
            context.Assert(motor.Position, 132, 138);

            await motor.StartSpeedForDegreesAsync(45, 10, 100);
            await Task.Delay(2000);
            context.Assert(Math.Abs(motor.AbsolutePosition), 177, 180);
            context.Assert(motor.Position, 177, 183);

            await context.ConfirmAsync("TachoMotor.StartSpeedForDegreesAsync: Has moved 40 times CW each by 45°?");
        }

        private static async Task TestCase2_TachoMotorExplicitPositionAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: Testing explicit position from TachoMotor.SetZero");

            await motor.GotoPositionAsync(360, 10, 100);
            await Task.Delay(2000);
            context.Assert(motor.AbsolutePosition, -3, 3);
            context.Assert(motor.Position, 357, 363);

            await context.ConfirmAsync("TachoMotor.GotoPositionAsync: has moved CW  to zero position?");

            await motor.GotoPositionAsync(810, 10, 100);
            await Task.Delay(4000);
            context.Assert(motor.AbsolutePosition, 87, 93);
            context.Assert(motor.Position, 807, 813);

            await context.ConfirmAsync("TachoMotor.GotoPositionAsync: Motor has moved CW by 360° + 90° and is exaclty 90° off zero?");
        }

        private static async Task TestCase3_TachoMotorHighSpeedAndFloatingAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: High different Speed and Floating");

            await motor.StartSpeedForDegreesAsync(810, -100, 100, SpecialSpeed.Float);
            await Task.Delay(2000);

            await context.ConfirmAsync("TachoMotor.StartSpeedForDegreesAsync: High speed CCW turn with floating end state?");

            await ResetToZeroAsync(context, motor, 2000);
        }

        private static async Task TestCase4_TachoMotorPositiveNegativeSpeedForTimeAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: Positive and Negative Speeds and for Time");

            await motor.StartSpeedForTimeAsync(1000, 10, 100);
            await Task.Delay(2000);

            await context.ConfirmAsync("TachoMotor.StartSpeedForTimeAsync: CW for 1s?");

            await motor.StartSpeedForTimeAsync(1000, -10, 100);
            await Task.Delay(2000);

            await context.ConfirmAsync("TachoMotor.StartSpeedForTimeAsync: CCW for 1s?");

            await ResetToZeroAsync(context, motor, 3000);
        }

        private static async Task TestCase5_TachoMotorAccelerationAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: Acceleration Profiles");

            await motor.SetAccelerationTimeAsync(500);
            await motor.SetDecelerationTimeAsync(500);

            await motor.StartSpeedForTimeAsync(2000, 50, 100, SpecialSpeed.Brake, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile);
            await Task.Delay(4000);

            await context.ConfirmAsync("TachoMotor.SetAccelerationTimeAsync: CW 0.5s each slow start and end?");

            await ResetToZeroAsync(context, motor, 20_000);
        }

        private static async Task TestCase6_BasicMotorAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("TachoMotor: Start Speed & Power");

            await motor.StartPowerAsync(100);
            await Task.Delay(1000);
            await motor.StopByBrakeAsync();
            await motor.StartPowerAsync(-100);
            await Task.Delay(1000);
            await motor.StopByFloatAsync();

            await context.ConfirmAsync("TachoMotor.StartPowerAsync: CW for 1s, brake, CCW for 1s, floating?");

            await ResetToZeroAsync(context, motor, 3000);
        }

        private async Task TestCase7_InputAsync(TestScriptExecutionContext context, TTechnicMotor motor)
        {
            context.Log.LogInformation("AbsoluteMotor: Input on Absolute and relative Position");

            context.Log.LogInformation("Turn 90° clockwise");

            await motor.AbsolutePositionObservable.Where(x => x.SI > 85 && x.SI < 95).FirstAsync().GetAwaiter();

            context.Log.LogInformation("Turn 180° counter-clockwise");

            await motor.AbsolutePositionObservable.Where(x => x.SI < -85 && x.SI > -95).FirstAsync().GetAwaiter();

            context.Log.LogInformation("Turn 90° counter-clockwise");

            await motor.PositionObservable.Where(x => x.SI < -175 && x.SI > -185).FirstAsync().GetAwaiter();
        }

        private static async Task ResetToZeroAsync(TestScriptExecutionContext context, TTechnicMotor motor, int expectedTime)
        {
            await motor.GotoPositionAsync(0, 10, 100);
            await Task.Delay(expectedTime);
            context.Assert(motor.AbsolutePosition, -3, 3);
            context.Assert(motor.Position, -3, 3);

            await context.ConfirmAsync("TachoMotor.GotoPositionAsync: has moved to zero position?");
        }
    }
}
