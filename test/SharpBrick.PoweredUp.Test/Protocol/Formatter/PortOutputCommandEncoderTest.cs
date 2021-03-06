using System;
using SharpBrick.PoweredUp.Devices;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortOutputCommandEncoderTest
    {

        [Theory]
        [InlineData("09-00-81-00-11-05-B8-0B-01", 0, 3000, SpeedProfiles.AccelerationProfile)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetAccTimeMessage(string expectedData, byte port, ushort time, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetAccTimeMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Time: time,
                Profile: profile
            ));

        [Theory]
        [InlineData("09-00-81-00-11-06-E8-03-02", 0, 1000, SpeedProfiles.DecelerationProfile)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetDecTimeMessage(string expectedData, byte port, ushort time, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetDecTimeMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Time: time,
                Profile: profile
            ));

        [Theory]
        [InlineData("09-00-81-00-11-07-64-5A-03", 0, 100, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("09-00-81-00-11-07-7F-5A-03", 0, 127, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("09-00-81-00-11-07-9C-5A-03", 0, -100, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("09-00-81-00-11-07-00-5A-03", 0, 0, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeedMessage(string expectedData, byte port, sbyte speed, byte maxPower, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeedMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Speed: speed,
                MaxPower: maxPower,
                Profile: profile
            ));

        [Theory]
        [InlineData("0A-00-81-00-11-08-64-64-5A-03", 0, 100, 100, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("0A-00-81-00-11-08-7F-7F-5A-03", 0, 127, 127, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("0A-00-81-00-11-08-9C-64-5A-03", 0, -100, 100, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        [InlineData("0A-00-81-00-11-08-00-64-5A-03", 0, 0, 100, 90, SpeedProfiles.AccelerationProfile | SpeedProfiles.DecelerationProfile)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeed2Message(string expectedData, byte port, sbyte speed1, sbyte speed2, byte maxPower, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeed2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Speed1: speed1,
                Speed2: speed2,
                MaxPower: maxPower,
                Profile: profile
            ));

        [Theory]
        [InlineData("0C-00-81-00-11-09-B8-0B-5A-64-7E-00", 0, 3000, 90, 100, SpecialSpeed.Hold, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeedForTimeMessage(string expectedData, byte port, ushort time, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeedForTimeMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Time: time,
                Speed: speed,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));

        [Theory]
        [InlineData("0D-00-81-00-11-0A-B8-0B-5A-64-64-7E-00", 0, 3000, 90, 100, 100, SpecialSpeed.Hold, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeedForTime2Message(string expectedData, byte port, ushort time, sbyte speed1, sbyte speed2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeedForTime2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Time: time,
                Speed1: speed1,
                Speed2: speed2,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));

        [Theory]
        [InlineData("0E-00-81-00-11-0B-B4-00-00-00-F6-64-7F-00", 0, 180, -10, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeedForDegreesMessage(string expectedData, byte port, uint degrees, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeedForDegreesMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees: degrees,
                Speed: speed,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));

        [Theory]
        [InlineData("0F-00-81-00-11-0C-B4-00-00-00-F6-64-64-7F-00", 0, 180, -10, 100, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartSpeedForDegrees2Message(string expectedData, byte port, uint degrees, sbyte speed1, sbyte speed2, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartSpeedForDegrees2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Degrees: degrees,
                Speed1: speed1,
                Speed2: speed2,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));

        [Theory]
        [InlineData("0E-00-81-00-11-0D-2D-00-00-00-0A-64-7F-00", 0, 45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        [InlineData("0E-00-81-00-11-0D-D3-FF-FF-FF-0A-64-7F-00", 0, -45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandGotoAbsolutePositionMessage(string expectedData, byte port, int absPosition, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandGotoAbsolutePositionMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                AbsolutePosition: absPosition,
                Speed: speed,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));


        [Theory]
        [InlineData("12-00-81-00-11-0E-2D-00-00-00-00-00-00-00-0A-64-7F-00", 0, 45, 0, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        [InlineData("12-00-81-00-11-0E-D3-FF-FF-FF-2D-00-00-00-0A-64-7F-00", 0, -45, 45, 10, 100, SpecialSpeed.Brake, SpeedProfiles.None)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandGotoAbsolutePosition2Message(string expectedData, byte port, int absPos1, int absPos2, sbyte speed, byte maxPower, SpecialSpeed endState, SpeedProfiles profile)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandGotoAbsolutePosition2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                AbsolutePosition1: absPos1,
                AbsolutePosition2: absPos2,
                Speed: speed,
                MaxPower: maxPower,
                EndState: endState,
                Profile: profile
            ));

        [Theory]
        [InlineData("08-00-81-00-11-51-00-64", 0, 100)]
        [InlineData("08-00-81-00-11-51-00-7F", 0, 127)]
        [InlineData("08-00-81-00-11-51-00-9C", 0, -100)]
        [InlineData("08-00-81-00-11-51-00-00", 0, 0)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartPowerMessage(string expectedData, byte port, sbyte power)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartPowerMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Power: power
            ));

        [Theory]
        [InlineData("08-00-81-00-11-02-64-0A", 0, 100, 10)]
        [InlineData("08-00-81-00-11-02-7F-0A", 0, 127, 10)]
        [InlineData("08-00-81-00-11-02-9C-0A", 0, -100, 10)]
        [InlineData("08-00-81-00-11-02-00-0A", 0, 0, 10)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandStartPower2Message(string expectedData, byte port, sbyte power1, sbyte power2)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandStartPower2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Power1: power1,
                Power2: power2
            ));

        [Theory]
        [InlineData("0A-00-81-32-11-51-01-00-FF-00", 50, 0x00, 0xFF, 0x00)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetRgbColorNo2Message(string expectedData, byte port, byte r, byte g, byte b)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetRgbColorNo2Message(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                r, g, b
            ));

        [Theory]
        [InlineData("08-00-81-32-11-51-00-05", 50, PoweredUpColor.Cyan)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetRgbColorNoMessage(string expectedData, byte port, PoweredUpColor color)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetRgbColorNoMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                ColorNo: color
            ));

        [Theory]
        [InlineData("0B-00-81-03-11-51-02-00-00-00-00", 3, 2, 0)]
        [InlineData("0B-00-81-01-11-51-03-0F-00-00-00", 1, 3, 15)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandPresetEncoderMessage(string expectedData, byte port, byte modeIndex, int position)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandPresetEncoderMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Position: position
            )
            {
                ModeIndex = modeIndex,
            });

        [Theory]
        [InlineData("0B-00-81-63-11-51-01-00-00-00-00", 99, 1, 0)]
        [InlineData("0B-00-81-63-11-51-01-0F-00-00-00", 99, 1, 15)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandTiltImpactPresetMessage(string expectedData, byte port, byte modeIndex, int presetValue)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandTiltImpactPresetMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                PresetValue: presetValue
            )
            {
                ModeIndex = modeIndex,
            });

        [Theory]
        [InlineData("09-00-81-63-11-51-02-02-03", 99, 2, 2, 3)]
        [InlineData("09-00-81-63-11-51-02-04-05", 99, 2, 4, 5)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandTiltConfigImpactMessage(string expectedData, byte port, byte modeIndex, sbyte impactThreshold, sbyte bumpHoldoff)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandTiltConfigImpactMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                ImpactThreshold: impactThreshold,
                BumpHoldoff: bumpHoldoff
            )
            {
                ModeIndex = modeIndex,
            });

        [Theory]
        [InlineData("08-00-81-63-11-51-02-00", 99, 2, TiltConfigOrientation.Bottom)]
        [InlineData("08-00-81-63-11-51-02-03", 99, 2, TiltConfigOrientation.Left)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandTiltConfigOrientationMessage(string expectedData, byte port, byte modeIndex, TiltConfigOrientation orientation)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandTiltConfigOrientationMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                Orientation: orientation
            )
            {
                ModeIndex = modeIndex,
            });

        [Theory]
        [InlineData("0A-00-81-63-11-51-02-01-02-03", 99, 2, new byte[] { 1, 2, 3 })]
        [InlineData("09-00-81-63-11-51-02-05-06", 99, 2, new byte[] { 5, 6 })]
        public void PortOutputCommandEncoder_Encode_GenericWriteDirectModeDataMessage(string expectedData, byte port, byte modeIndex, byte[] data)
            => PortOutputCommandEncoder_Encode(expectedData, new GenericWriteDirectModeDataMessage(
                port,
                PortOutputCommandStartupInformation.ExecuteImmediately, PortOutputCommandCompletionInformation.CommandFeedback,
                modeIndex,
                data
            ));

        private void PortOutputCommandEncoder_Encode(string expectedDataAsString, LegoWirelessMessage message)
        {
            // act
            var data = MessageEncoder.Encode(message, null);

            // assert
            Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
        }
    }
}