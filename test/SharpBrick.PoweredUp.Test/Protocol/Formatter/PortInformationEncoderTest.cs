using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInformationEncoderTest
    {
        [Theory]
        [InlineData(2, PortInformationType.ModeInfo, true, true, true, true, 3, 0x0005, 0x0006, "0B-00-43-02-01-0F-03-05-00-06-00")]
        [InlineData(0, PortInformationType.ModeInfo, true, true, true, true, 6, 0x001E, 0x001F, "0B-00-43-00-01-0F-06-1E-00-1F-00")] // Technic Large Motor
        // identical to line above [InlineData(0, PortInformationType.ModeInfo, true, true, true, true, 6, 0x001E, 0x001F, "0B-00-43-00-01-0F-06-1E-00-1F-00")] // Technic XLarge Motor
        public void PortInformationEncoder_Decode_PortInformationForModeInfoMessage(byte expectedPort, PortInformationType expectedType, bool expectedOutput, bool expectedInput, bool expectedLogicalCombinable, bool expectedLogicalSynchronizable, byte expectedTotalCount, ushort expectedInputModes, ushort expectedOutputModes, string dataAsString)
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, null) as PortInformationForModeInfoMessage;

            // assert
            Assert.Equal(expectedPort, message.PortId);
            Assert.Equal(expectedType, message.InformationType);
            Assert.Equal(expectedOutput, message.OutputCapability);
            Assert.Equal(expectedInput, message.InputCapability);
            Assert.Equal(expectedLogicalCombinable, message.LogicalCombinableCapability);
            Assert.Equal(expectedLogicalSynchronizable, message.LogicalSynchronizableCapability);
            Assert.Equal(expectedTotalCount, message.TotalModeCount);
            Assert.Equal(expectedInputModes, message.InputModes);
            Assert.Equal(expectedOutputModes, message.OutputModes);
        }


        [Theory]
        [InlineData(3, PortInformationType.PossibleModeCombinations, new ushort[] { 0x000E, 0x0003 }, "09-00-43-03-02-0E-00-03-00")]
        [InlineData(0, PortInformationType.PossibleModeCombinations, new ushort[] { 0b0000_0000_0000_1110 }, "07-00-43-00-02-0E-00")] // Technic Large Motor
        // identical to line above [InlineData(0, PortInformationType.PossibleModeCombinations, new ushort[] { 0b0000_0000_0000_1110 }, "07-00-43-00-02-0E-00")] // Technic XLarge Motor
        public void PortInformationEncoder_Decode_PortInformationForPossibleModeCombinationsMessage(byte expectedPort, PortInformationType expectedType, ushort[] expectedCombinations, string dataAsString)
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, null) as PortInformationForPossibleModeCombinationsMessage;

            // assert
            Assert.Equal(expectedPort, message.PortId);
            Assert.Equal(expectedType, message.InformationType);
            Assert.Collection(message.ModeCombinations, expectedCombinations.Select<ushort, Action<ushort>>(combination => { return (item) => Assert.Equal(combination, item); }).ToArray());
        }
    }
}