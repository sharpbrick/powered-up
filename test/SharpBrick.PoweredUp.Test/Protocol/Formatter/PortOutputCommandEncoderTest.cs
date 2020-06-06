using System;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortOutputCommandEncoderTest
    {
        [Theory]
        [InlineData("0A-00-81-32-11-51-01-00-FF-00", 50, 0x00, 0xFF, 0x00)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetRgbColorNo2Message(string expectedData, byte port, byte r, byte g, byte b)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetRgbColorNo2Message()
            {
                PortId = port,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                RedColor = r,
                GreenColor = g,
                BlueColor = b,
            });

        [Theory]
        [InlineData("08-00-81-32-11-51-00-05", 50, PortOutputCommandColors.Cyan)]
        public void PortOutputCommandEncoder_Encode_PortOutputCommandSetRgbColorNoMessage(string expectedData, byte port, PortOutputCommandColors color)
            => PortOutputCommandEncoder_Encode(expectedData, new PortOutputCommandSetRgbColorNoMessage()
            {
                PortId = port,
                StartupInformation = PortOutputCommandStartupInformation.ExecuteImmediately,
                CompletionInformation = PortOutputCommandCompletionInformation.CommandFeedback,
                ColorNo = color,
            });

        private void PortOutputCommandEncoder_Encode(string expectedDataAsString, PoweredUpMessage message)
        {
            // act
            var data = MessageEncoder.Encode(message, null);

            // assert
            Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
        }
    }
}