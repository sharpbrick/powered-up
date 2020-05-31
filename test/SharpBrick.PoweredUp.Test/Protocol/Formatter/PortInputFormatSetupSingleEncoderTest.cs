using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInputFormatSetupSingleEncoderTest
    {
        [Theory]
        [InlineData("0A-00-41-00-02-2D-00-00-00-01", 0x00, 0x02, 45, true)]
        public void PortInputFormatSetupSingleEncoder_Decode(string expectedDataAsString, byte portId, byte mode, uint deltaInterval, bool notificationEnabled)
        {
            // arrange
            var message = new PortInputFormatSetupSingleMessage()
            {
                PortId = portId,
                Mode = mode,
                DeltaInterval = deltaInterval,
                NotificationEnabled = notificationEnabled,
            };

            // act
            var data = MessageEncoder.Encode(message);

            // assert
            Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
        }
    }
}