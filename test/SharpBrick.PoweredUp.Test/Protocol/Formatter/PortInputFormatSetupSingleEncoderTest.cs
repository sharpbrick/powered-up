using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortInputFormatSetupSingleEncoderTest
{
    [Theory]
    [InlineData("0A-00-41-00-02-2D-00-00-00-01", 0x00, 0x02, 45, true)]
    public void PortInputFormatSetupSingleEncoder_Encode(string expectedDataAsString, byte portId, byte mode, uint deltaInterval, bool notificationEnabled)
    {
        // arrange
        var message = new PortInputFormatSetupSingleMessage(portId, mode, deltaInterval, notificationEnabled);

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedDataAsString, BytesStringUtil.DataToString(data));
    }
}
