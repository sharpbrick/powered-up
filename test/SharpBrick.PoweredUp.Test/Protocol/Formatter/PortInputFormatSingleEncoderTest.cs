using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInputFormatSingleEncoderTest
    {
        [Theory]
        [InlineData("0A-00-47-00-02-2D-00-00-00-01", 0x00, 0x02, 45, true)]
        public void PortInputFormatSingleEncoder_Decode(string dataAsString, byte expectedPortId, byte expectedMode, uint expectedDeltaInterval, bool expectedNotificationEnabled)
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, null) as PortInputFormatSingleMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Equal(expectedMode, message.ModeIndex);
            Assert.Equal(expectedDeltaInterval, message.DeltaInterval);
            Assert.Equal(expectedNotificationEnabled, message.NotificationEnabled);

            // reverse
            var reverseMessage = new PortInputFormatSingleMessage(expectedPortId, expectedMode, expectedDeltaInterval, expectedNotificationEnabled)
            {
                HubId = 0,
            };

            var reverseData = MessageEncoder.Encode(reverseMessage, null);
            var reverseDataAsString = BytesStringUtil.DataToString(reverseData);

            Assert.Equal(dataAsString, reverseDataAsString);
        }
    }
}