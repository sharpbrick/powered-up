using System;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class LwpVersionNumberEncoderTest
{
    [Theory]
    [InlineData(0x12, 0x23, "12.23")]
    [InlineData(0x01, 0x00, "1.0")]
    public void LwpVersionNumberEncoding_DecodeVersion(byte byte1, byte byte0, string expectedVersionAsString)
    {
        var actualVersion = LwpVersionNumberEncoder.Decode(byte0, byte1);

        Assert.Equal(Version.Parse(expectedVersionAsString), actualVersion);
    }
}
