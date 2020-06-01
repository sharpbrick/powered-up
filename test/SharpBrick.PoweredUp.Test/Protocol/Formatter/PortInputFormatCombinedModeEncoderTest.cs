using System;
using System.Linq;
using SharpBrick.PoweredUp.Protocol.Messages;
using SharpBrick.PoweredUp.Utils;
using Xunit;

namespace SharpBrick.PoweredUp.Protocol.Formatter
{
    public class PortInputFormatCombinedModeEncoderTest
    {
        [Theory]
        [InlineData("07-00-48-00-80-07-00", 0x00, 0x00, true, new int[] { 0, 1, 2 })]
        public void PortInputFormatCombinedModeEncoder_Decode(string dataAsString, byte expectedPortId, byte expectedCombinationIndex, bool expectedEnabled, int[] expectedIndexes)
        {
            // arrange
            var data = BytesStringUtil.StringToData(dataAsString);

            // act
            var message = MessageEncoder.Decode(data, null) as PortInputFormatCombinedModeMessage;

            // assert
            Assert.Equal(expectedPortId, message.PortId);
            Assert.Equal(expectedCombinationIndex, message.UsedCombinationIndex);
            Assert.Equal(expectedEnabled, message.MultiUpdateEnabled);
            Assert.Collection(message.ConfiguredModeDataSetIndex, expectedIndexes.Select<int, Action<int>>(exp => (act) => Assert.Equal(exp, act)).ToArray());
        }
    }
}