﻿using System;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter;

public class VersionNumberEncoderTest
{
    [Theory]
    [InlineData(0b0001_0000___0000_0000___0000_0000___0000_0000, "1.0.0.0")]
    [InlineData(0b0010_0000___0000_0000___0000_0000___0000_0000, "2.0.0.0")]
    [InlineData(0b0101_0000___0000_0000___0000_0000___0000_0000, "5.0.0.0")]
    [InlineData(0b0111_0000___0000_0000___0000_0000___0000_0000, "7.0.0.0")]
    [InlineData(0b0000_0101___0000_0000___0000_0000___0000_0000, "0.5.0.0")]
    [InlineData(0b0000_1101___0000_0000___0000_0000___0000_0000, "0.13.0.0")]
    [InlineData(0b0111_1101___0000_0000___0000_0000___0000_0000, "7.13.0.0")]
    [InlineData(0x78_00_00_00, "7.8.0.0")]
    [InlineData(0x60_00_00_00, "6.0.0.0")]
    [InlineData(0x1_7_37_1510, "1.7.37.1510")]
    public void PropertyVersionNumberEncoding_DecodeVersion(int input, string expectedVersionAsString)
    {
        var expected = Version.Parse(expectedVersionAsString);
        var actual = VersionNumberEncoder.Decode(input);

        Assert.Equal(expected, actual);

        var x = VersionNumberEncoder.Encode(expected);

        Assert.Equal(input, x);
    }
}
