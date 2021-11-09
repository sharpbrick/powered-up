using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class PortModeInformationRequestEncoderTest
{

    [Theory]
    [InlineData(2, 8, PortModeInformationType.Name, "06-00-22-02-08-00")]
    [InlineData(2, 8, PortModeInformationType.Raw, "06-00-22-02-08-01")]
    [InlineData(2, 8, PortModeInformationType.Pct, "06-00-22-02-08-02")]
    [InlineData(2, 8, PortModeInformationType.SI, "06-00-22-02-08-03")]
    [InlineData(2, 8, PortModeInformationType.Symbol, "06-00-22-02-08-04")]
    [InlineData(2, 8, PortModeInformationType.Mapping, "06-00-22-02-08-05")]
    [InlineData(2, 8, PortModeInformationType.InternalUse, "06-00-22-02-08-06")]
    [InlineData(2, 8, PortModeInformationType.MotorBias, "06-00-22-02-08-07")]
    [InlineData(2, 8, PortModeInformationType.CapabilityBits, "06-00-22-02-08-08")]
    [InlineData(2, 8, PortModeInformationType.ValueFormat, "06-00-22-02-08-80")]

    [InlineData(3, 2, PortModeInformationType.Name, "06-00-22-03-02-00")]
    public void PortModeInformationRequestEncoder_Encode(byte portId, byte mode, PortModeInformationType informationType, string expectedData)
    {
        // arrange
        var message = new PortModeInformationRequestMessage(portId, mode, informationType);

        // act
        var data = MessageEncoder.Encode(message, null);

        // assert
        Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
    }
}
