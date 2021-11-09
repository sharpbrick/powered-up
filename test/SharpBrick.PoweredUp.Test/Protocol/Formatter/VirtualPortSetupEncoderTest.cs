using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class VirtualPortSetupEncoderTest
{

    [Theory]
    [InlineData(0x01, 0x02, "06-00-61-01-01-02")]
    public void PortModeInformationRequestEncoder_Encode_Connected(byte portA, byte portB, string expectedData)
    {
        // arrange
        var message = new VirtualPortSetupForConnectedMessage(portA, portB);

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
    }

    [Theory]
    [InlineData(0x88, "05-00-61-00-88")]
    public void PortModeInformationRequestEncoder_Encode_Disconnected(byte port, string expectedData)
    {
        // arrange
        var message = new VirtualPortSetupForDisconnectedMessage(port);

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
    }
}
