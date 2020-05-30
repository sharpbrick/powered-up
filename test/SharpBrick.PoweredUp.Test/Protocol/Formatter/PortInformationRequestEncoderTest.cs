using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInformationRequestEncoderTest
    {

        [Theory]
        [InlineData(2, PortInformationType.PortValue, "05-00-21-02-00")]
        [InlineData(2, PortInformationType.ModeInfo, "05-00-21-02-01")]
        [InlineData(2, PortInformationType.PossibleModeCombinations, "05-00-21-02-02")]
        [InlineData(3, PortInformationType.PortValue, "05-00-21-03-00")]
        public void PortInformationRequestEncoder_Encode(byte portId, PortInformationType informationType, string expectedData)
        {
            // arrange
            var message = new PortInformationRequestMessage() { PortId = portId, InformationType = informationType };

            // act
            var data = MessageEncoder.Encode(message);

            // assert
            Assert.Equal(expectedData, BytesStringUtil.DataToString(data));
        }
    }
}