using System;
using SharpBrick.PoweredUp.Protocol.Formatter;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class CommonMessageHeaderEncoderTest
    {
        [Theory]
        [InlineData(new byte[] { 0b0000_1001, 0b0000_0100, 0b0000_0101 }, 9, 4, 5, 3)]
        [InlineData(new byte[] { 0b0111_1111, 0b0000_0100, 0b0000_0101 }, 127, 4, 5, 3)]
        [InlineData(new byte[] { 0b1000_0000, 0b0000_0001, 0b0000_0100, 0b0000_0101 }, 128, 4, 5, 4)]
        [InlineData(new byte[] { 0b1000_0001, 0b0000_0001, 0b0000_0100, 0b0000_0101 }, 129, 4, 5, 4)]
        [InlineData(new byte[] { 0b1000_0010, 0b0000_0001, 0b0000_0100, 0b0000_0101 }, 130, 4, 5, 4)]
        public void CommonMessageHeaderParser_Parse(byte[] data, ushort expectedLength, byte expectedHubId, byte expectedMessageType, ushort expectedHeaderLength)
        {
            var (length, hubId, messageType, headerLength) = CommonMessageHeaderEncoder.ParseCommonHeader(data);

            Assert.Equal(expectedLength, length);
            Assert.Equal(expectedHubId, hubId);
            Assert.Equal(expectedMessageType, messageType);
            Assert.Equal(expectedHeaderLength, headerLength);
        }
    }
}
